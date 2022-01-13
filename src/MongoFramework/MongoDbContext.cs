using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Commands;
using MongoFramework.Infrastructure.Indexing;
using MongoFramework.Infrastructure.Internal;
using MongoFramework.Infrastructure.Linq;
using MongoFramework.Utilities;

namespace MongoFramework
{
	public class MongoDbContext : IMongoDbContext, IDisposable
	{
		public IMongoDbConnection Connection { get; }

		public EntityEntryContainer ChangeTracker { get; } = new EntityEntryContainer();
		public EntityCommandStaging CommandStaging { get; } = new EntityCommandStaging();

		public MongoDbContext(IMongoDbConnection connection)
		{
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

		protected virtual void AfterDetectChanges() { }

		protected virtual WriteModelOptions GetWriteModelOptions() => WriteModelOptions.Default;

		public virtual void SaveChanges()
		{
			ChangeTracker.DetectChanges();
			AfterDetectChanges();
			var commands = GenerateWriteCommands();
			var writeModelOptions = GetWriteModelOptions();

			var commandsByEntityType = commands.GroupBy(c => c.EntityType);
			foreach (var entityTypeCommands in commandsByEntityType)
			{
				var method = GenericsHelper.GetMethodDelegate<Action<IMongoDbConnection, IEnumerable<IWriteCommand>, WriteModelOptions>>(
					typeof(MongoDbContext), nameof(InternalSaveChanges), entityTypeCommands.Key
				);
				method(Connection, entityTypeCommands, writeModelOptions);
			}

			ChangeTracker.CommitChanges();
			CommandStaging.CommitChanges();
		}

		private static void InternalSaveChanges<TEntity>(IMongoDbConnection connection, IEnumerable<IWriteCommand> commands, WriteModelOptions options) where TEntity : class
		{
			EntityIndexWriter.ApplyIndexing<TEntity>(connection);
			EntityCommandWriter.Write<TEntity>(connection, commands, options);
		}

		public virtual async Task SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			ChangeTracker.DetectChanges();
			AfterDetectChanges();
			var commands = GenerateWriteCommands();
			var writeModelOptions = GetWriteModelOptions();

			var commandsByEntityType = commands.GroupBy(c => c.EntityType);
			foreach (var entityTypeCommands in commandsByEntityType)
			{
				var methodAsync = GenericsHelper.GetMethodDelegate<Func<IMongoDbConnection, IEnumerable<IWriteCommand>, WriteModelOptions, CancellationToken, Task>>(
					typeof(MongoDbContext), nameof(InternalSaveChangesAsync), entityTypeCommands.Key
				);
				await methodAsync(Connection, entityTypeCommands, writeModelOptions, cancellationToken);
			}

			ChangeTracker.CommitChanges();
			CommandStaging.CommitChanges();
		}

		private static async Task InternalSaveChangesAsync<TEntity>(IMongoDbConnection connection, IEnumerable<IWriteCommand> commands, WriteModelOptions options, CancellationToken cancellationToken) where TEntity : class
		{
			await EntityIndexWriter.ApplyIndexingAsync<TEntity>(connection);
			await EntityCommandWriter.WriteAsync<TEntity>(connection, commands, options, cancellationToken);
		}

		public virtual IMongoDbSet<TEntity> Set<TEntity>() where TEntity : class
		{
			var properties = DbSetInitializer.GetDbSetProperties(this);
			var existing = properties.FirstOrDefault(p => p.PropertyType.GenericTypeArguments[0] == typeof(TEntity));

			if (existing != null)
			{
				return existing.GetValue(this) as IMongoDbSet<TEntity>;
			}

			return new MongoDbSet<TEntity>(this);
		}

		public IQueryable<TEntity> Query<TEntity>() where TEntity : class
		{
			var provider = new MongoFrameworkQueryProvider<TEntity>(Connection);
			return new MongoFrameworkQueryable<TEntity>(provider);
		}

		/// <summary>
		/// Marks the entity as unchanged in the change tracker and starts tracking.
		/// </summary>
		/// <param name="entity"></param>
		public virtual void Attach<TEntity>(TEntity entity) where TEntity : class
		{
			Check.NotNull(entity, nameof(entity));
			ChangeTracker.SetEntityState(entity, EntityEntryState.NoChanges);
		}

		/// <summary>
		/// Marks the collection of entities as unchanged in the change tracker and starts tracking.
		/// </summary>
		/// <param name="entities"></param>
		public virtual void AttachRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
		{
			Check.NotNull(entities, nameof(entities));
			foreach (var entity in entities)
			{
				ChangeTracker.SetEntityState(entity, EntityEntryState.NoChanges);
			}
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
