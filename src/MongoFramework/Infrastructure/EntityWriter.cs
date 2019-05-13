using MongoDB.Driver;
using MongoFramework.Infrastructure.DefinitionHelpers;
using MongoFramework.Infrastructure.Diagnostics;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mutation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure
{
	public class EntityWriter<TEntity> : IEntityWriter<TEntity> where TEntity : class
	{
		public IMongoDbConnection Connection { get; }
		private IEntityDefinition EntityDefinition { get; }
		
		public EntityWriter(IMongoDbConnection connection)
		{
			Connection = connection ?? throw new ArgumentNullException(nameof(connection));
			EntityDefinition = EntityMapping.GetOrCreateDefinition(typeof(TEntity));
		}

		private IMongoCollection<TEntity> GetCollection()
		{
			return Connection.GetDatabase().GetCollection<TEntity>(EntityDefinition.CollectionName);
		}

		private IEnumerable<WriteModel<TEntity>> BuildWriteModel(IEntityCollection<TEntity> entityCollection)
		{
			var idFieldName = EntityDefinition.GetIdName();
			var writeModel = new List<WriteModel<TEntity>>();

			foreach (var entry in entityCollection.GetEntries())
			{
				if (entry.State == EntityEntryState.Added)
				{
					EntityMutation<TEntity>.MutateEntity(entry.Entity, MutatorType.Insert, Connection);
					writeModel.Add(new InsertOneModel<TEntity>(entry.Entity));
				}
				else if (entry.State == EntityEntryState.Updated)
				{
					EntityMutation<TEntity>.MutateEntity(entry.Entity, MutatorType.Update, Connection);
					var idFieldValue = EntityDefinition.GetIdValue(entry.Entity);
					var filter = Builders<TEntity>.Filter.Eq(idFieldName, idFieldValue);
					var updateDefintion = UpdateDefinitionHelper.CreateFromDiff<TEntity>(entry.OriginalValues, entry.CurrentValues);

					//MongoDB doesn't like it if an UpdateDefinition is empty.
					//This is primarily to work around a mutation that may set an entity to its default state.
					if (updateDefintion.HasChanges())
					{
						writeModel.Add(new UpdateOneModel<TEntity>(filter, updateDefintion));
					}
				}
				else if (entry.State == EntityEntryState.Deleted)
				{
					var idFieldValue = EntityDefinition.GetIdValue(entry.Entity);
					var filter = Builders<TEntity>.Filter.Eq(idFieldName, idFieldValue);
					writeModel.Add(new DeleteOneModel<TEntity>(filter));
				}
			}

			return writeModel;
		}

		public void Write(IEntityCollection<TEntity> entityCollection)
		{
			var writeModel = BuildWriteModel(entityCollection);

			if (writeModel.Any())
			{
				var commandId = Guid.NewGuid();
				try
				{
					Connection.DiagnosticListener.OnNext(new WriteDiagnosticCommand<TEntity>
					{
						CommandId = commandId,
						Source = $"{nameof(EntityWriter<TEntity>)}.{nameof(Write)}",
						CommandState = CommandState.Start,
						EntityType = typeof(TEntity),
						WriteModel = writeModel
					});
					GetCollection().BulkWrite(writeModel);
					Connection.DiagnosticListener.OnNext(new WriteDiagnosticCommand<TEntity>
					{
						CommandId = commandId,
						Source = $"{nameof(EntityWriter<TEntity>)}.{nameof(Write)}",
						CommandState = CommandState.End,
						EntityType = typeof(TEntity),
						WriteModel = writeModel
					});
				}
				catch (Exception ex)
				{
					Connection.DiagnosticListener.OnNext(new WriteDiagnosticCommand<TEntity>
					{
						CommandId = commandId,
						Source = $"{nameof(EntityWriter<TEntity>)}.{nameof(Write)}",
						CommandState = CommandState.Error,
						EntityType = typeof(TEntity),
						WriteModel = writeModel
					});
					Connection.DiagnosticListener.OnError(ex);

					throw;
				}
			}
		}

		public async Task WriteAsync(IEntityCollection<TEntity> entityCollection, CancellationToken cancellationToken = default(CancellationToken))
		{
			var writeModel = BuildWriteModel(entityCollection);

			cancellationToken.ThrowIfCancellationRequested();

			if (writeModel.Any())
			{
				var commandId = Guid.NewGuid();
				try
				{
					Connection.DiagnosticListener.OnNext(new WriteDiagnosticCommand<TEntity>
					{
						CommandId = commandId,
						Source = $"{nameof(EntityWriter<TEntity>)}.{nameof(WriteAsync)}",
						CommandState = CommandState.Start,
						EntityType = typeof(TEntity),
						WriteModel = writeModel
					});
					await GetCollection().BulkWriteAsync(writeModel, null, cancellationToken).ConfigureAwait(false);
					Connection.DiagnosticListener.OnNext(new WriteDiagnosticCommand<TEntity>
					{
						CommandId = commandId,
						Source = $"{nameof(EntityWriter<TEntity>)}.{nameof(WriteAsync)}",
						CommandState = CommandState.End,
						EntityType = typeof(TEntity),
						WriteModel = writeModel
					});
				}
				catch (Exception ex)
				{
					Connection.DiagnosticListener.OnNext(new WriteDiagnosticCommand<TEntity>
					{
						CommandId = commandId,
						Source = $"{nameof(EntityWriter<TEntity>)}.{nameof(WriteAsync)}",
						CommandState = CommandState.Error,
						EntityType = typeof(TEntity),
						WriteModel = writeModel
					});
					Connection.DiagnosticListener.OnError(ex);

					throw;
				}
			}
		}
	}
}
