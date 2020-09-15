using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Commands;
using MongoFramework.Infrastructure.Indexing;
using MongoFramework.Infrastructure.Internal;
using MongoFramework.Infrastructure.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MongoFramework
{
	public class MongoDbContext : IMongoDbContext, IDisposable
	{
		public IMongoDbConnection Connection { get; }

		public EntityEntryContainer ChangeTracker { get; } = new EntityEntryContainer();
		public EntityCommandStaging CommandStaging { get; } = new EntityCommandStaging();

		public virtual string TenantId { get; protected set; }

		public MongoDbContext(IMongoDbConnection connection, string tenantId = null)
		{
			TenantId = tenantId;
			Connection = connection;
			InitialiseDbSets();
		}

		private void InitialiseDbSets()
		{
			var properties = DbSetInitializer.GetDbSetProperties(this);
			foreach (var property in properties)
			{
				var dbSetOptions = DbSetInitializer.GetDefaultDbSetOptions(property);
				var dbSet = OnDbSetCreation(property, dbSetOptions);
				property.SetValue(this, dbSet);
			}
		}

		protected virtual IMongoDbSet OnDbSetCreation(PropertyInfo property, IDbSetOptions dbSetOptions)
		{
			return DbSetInitializer.CreateDbSet(property.PropertyType, this, dbSetOptions);
		}

		private IEnumerable<IWriteCommand> GenerateWriteCommands()
		{
			var entries = ChangeTracker.Entries();
			foreach (var entry in entries)
			{
				if (entry.State != EntityEntryState.NoChanges)
				{
					yield return EntityCommandBuilder.CreateCommand(entry);
				}
			}

			foreach (var command in CommandStaging.GetCommands())
			{
				yield return command;
			}
		}

		public virtual void SaveChanges()
		{
			ChangeTracker.DetectChanges();
			var commands = GenerateWriteCommands();

			var commandsByEntityType = commands.GroupBy(c => c.EntityType);
			foreach (var entityTypeCommands in commandsByEntityType)
			{
				var method = GenericsHelper.GetMethodDelegate<Action<IMongoDbConnection, IEnumerable<IWriteCommand>>>(
					typeof(MongoDbContext), nameof(InternalSaveChanges), entityTypeCommands.Key
				);
				method(Connection, entityTypeCommands);
			}

			ChangeTracker.CommitChanges();
			CommandStaging.CommitChanges();
		}

		private static void InternalSaveChanges<TEntity>(IMongoDbConnection connection, IEnumerable<IWriteCommand> commands) where TEntity : class
		{
			EntityIndexWriter.ApplyIndexing<TEntity>(connection);
			EntityCommandWriter.Write<TEntity>(connection, commands);
		}

		public virtual async Task SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			ChangeTracker.DetectChanges();
			var commands = GenerateWriteCommands();

			var commandsByEntityType = commands.GroupBy(c => c.EntityType);
			foreach (var entityTypeCommands in commandsByEntityType)
			{
				var methodAsync = GenericsHelper.GetMethodDelegate<Func<IMongoDbConnection, IEnumerable<IWriteCommand>, CancellationToken, Task>>(
					typeof(MongoDbContext), nameof(InternalSaveChangesAsync), entityTypeCommands.Key
				);
				await methodAsync(Connection, entityTypeCommands, cancellationToken);
			}

			ChangeTracker.CommitChanges();
			CommandStaging.CommitChanges();
		}
		private static async Task InternalSaveChangesAsync<TEntity>(IMongoDbConnection connection, IEnumerable<IWriteCommand> commands, CancellationToken cancellationToken) where TEntity : class
		{
			await EntityIndexWriter.ApplyIndexingAsync<TEntity>(connection);
			await EntityCommandWriter.WriteAsync<TEntity>(connection, commands, cancellationToken);
		}

		public IMongoDbSet<TEntity> Set<TEntity>() where TEntity : class
		{
			return new MongoDbSet<TEntity>(this);
		}

		public IQueryable<TEntity> Query<TEntity>() where TEntity : class
		{
			var provider = new MongoFrameworkQueryProvider<TEntity>(Connection);
			return new MongoFrameworkQueryable<TEntity>(provider);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				Connection?.Dispose();
			}
		}

		~MongoDbContext()
		{
			Dispose(false);
		}
	}
}
