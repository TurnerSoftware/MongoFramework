using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoFramework.Infrastructure.Commands;
using MongoFramework.Infrastructure.Mutation;

namespace MongoFramework.Infrastructure
{
	public class EntityWriterPipeline<TEntity> : IEntityWriterPipeline<TEntity> where TEntity : class
	{
		public IMongoDbConnection Connection { get; }
		private ICommandWriter<TEntity> CommandWriter { get; }
		private HashSet<IEntityCollectionBase<TEntity>> ManagedCollections { get; } = new HashSet<IEntityCollectionBase<TEntity>>();
		private HashSet<IWriteCommand<TEntity>> StagedCommands { get; } = new HashSet<IWriteCommand<TEntity>>();

		public EntityWriterPipeline(IMongoDbConnection connection)
		{
			Connection = connection;
			CommandWriter = new CommandWriter<TEntity>(connection);
		}

		public void AddCollection(IEntityCollectionBase<TEntity> collection)
		{
			if (collection == null)
			{
				throw new ArgumentNullException(nameof(collection));
			}

			if (!ManagedCollections.Contains(collection))
			{
				ManagedCollections.Add(collection);
			}
		}
		public void RemoveCollection(IEntityCollectionBase<TEntity> collection)
		{
			if (collection == null)
			{
				throw new ArgumentNullException(nameof(collection));
			}

			ManagedCollections.Remove(collection);
		}

		public void StageCommand(IWriteCommand<TEntity> command)
		{
			if (command == null)
			{
				throw new ArgumentNullException(nameof(command));
			}

			StagedCommands.Add(command);
		}
		public void ClearStaging() => StagedCommands.Clear();

		private void DetectChanges()
		{
			foreach (var collection in ManagedCollections)
			{
				var entries = collection.GetEntries();

				foreach (var entry in entries)
				{
					if (entry.State == EntityEntryState.NoChanges || entry.State == EntityEntryState.Updated)
					{
						entry.State = entry.HasChanges() ? EntityEntryState.Updated : EntityEntryState.NoChanges;
					}
				}
			}
		}
		private void CommitChanges()
		{
			foreach (var collection in ManagedCollections)
			{
				var entries = collection.GetEntries()
					.Where(e => e.State != EntityEntryState.NoChanges)
					.ToArray();

				foreach (var entry in entries)
				{
					if (entry.State == EntityEntryState.Added || entry.State == EntityEntryState.Updated)
					{
						entry.Refresh();
					}
					else if (entry.State == EntityEntryState.Deleted)
					{
						collection.Remove(entry.Entity);
					}
				}
			}
		}

		private IEnumerable<IWriteCommand<TEntity>> GetWriteCommands()
		{
			var results = new List<IWriteCommand<TEntity>>();

			foreach (var collection in ManagedCollections)
			{
				foreach (var entry in collection.GetEntries())
				{
					if (entry.State == EntityEntryState.Added)
					{
						EntityMutation<TEntity>.MutateEntity(entry.Entity, MutatorType.Insert, Connection);
					}
					else if (entry.State == EntityEntryState.Updated)
					{
						EntityMutation<TEntity>.MutateEntity(entry.Entity, MutatorType.Update, Connection);
					}

					var validationContext = new ValidationContext(entry.Entity);
					Validator.ValidateObject(entry.Entity, validationContext);

					var command = EntityCommandBuilder<TEntity>.CreateCommand(entry);
					if (command != null)
					{
						results.Add(command);
					}
				}
			}

			results.AddRange(StagedCommands);
			return results;
		}

		public void Write()
		{
			DetectChanges();
			var commands = GetWriteCommands();
			CommandWriter.Write(commands);
			CommitChanges();
			ClearStaging();
		}

		public async Task WriteAsync(CancellationToken cancellationToken = default)
		{
			DetectChanges();
			cancellationToken.ThrowIfCancellationRequested();
			var commands = GetWriteCommands();
			cancellationToken.ThrowIfCancellationRequested();
			await CommandWriter.WriteAsync(commands, cancellationToken).ConfigureAwait(false);
			CommitChanges();
			ClearStaging();
		}
	}
}
