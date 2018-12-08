using MongoDB.Driver;
using MongoFramework.Infrastructure.DefinitionHelpers;
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
		public IEntityMapper EntityMapper { get; }
		
		public EntityWriter(IMongoDbConnection connection)
		{
			Connection = connection ?? throw new ArgumentNullException(nameof(connection));
			EntityMapper = connection.GetEntityMapper(typeof(TEntity));
		}

		private IMongoCollection<TEntity> GetCollection()
		{
			var collectionName = EntityMapper.GetCollectionName();
			return Connection.GetDatabase().GetCollection<TEntity>(collectionName);
		}

		private IEnumerable<WriteModel<TEntity>> BuildWriteModel(IEntityCollection<TEntity> entityCollection)
		{
			var idFieldName = EntityMapper.GetIdName();
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
					var idFieldValue = EntityMapper.GetIdValue(entry.Entity);
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
					var idFieldValue = EntityMapper.GetIdValue(entry.Entity);
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
				//TODO: Add support for Transactions with MongoDB Server 4.0
				GetCollection().BulkWrite(writeModel);
			}
		}

		public async Task WriteAsync(IEntityCollection<TEntity> entityCollection, CancellationToken cancellationToken = default(CancellationToken))
		{
			var writeModel = BuildWriteModel(entityCollection);

			cancellationToken.ThrowIfCancellationRequested();

			if (writeModel.Any())
			{
				//TODO: Add support for Transactions with MongoDB Server 4.0
				await GetCollection().BulkWriteAsync(writeModel, null, cancellationToken).ConfigureAwait(false);
			}
		}
	}
}
