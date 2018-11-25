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
	public class DbEntityWriter<TEntity> : IDbEntityWriter<TEntity> where TEntity : class
	{
		public IMongoDatabase Database { get; private set; }
		public IEntityMapper EntityMapper { get; private set; }

		public DbEntityWriter(IMongoDatabase database) : this(database, new EntityMapper(typeof(TEntity))) { }

		public DbEntityWriter(IMongoDatabase database, IEntityMapper mapper)
		{
			Database = database ?? throw new ArgumentNullException(nameof(database));
			EntityMapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
		}

		private IMongoCollection<TEntity> GetCollection()
		{
			var collectionName = EntityMapper.GetCollectionName();
			return Database.GetCollection<TEntity>(collectionName);
		}

		private IEnumerable<WriteModel<TEntity>> BuildWriteModel(IDbEntityCollection<TEntity> entityCollection)
		{
			var idFieldName = EntityMapper.GetIdName();
			var writeModel = new List<WriteModel<TEntity>>();

			foreach (var entry in entityCollection.GetEntries())
			{
				if (entry.State == DbEntityEntryState.Added)
				{
					EntityMutation<TEntity>.MutateEntity(entry.Entity, MutatorType.Insert, Database);
					writeModel.Add(new InsertOneModel<TEntity>(entry.Entity));
				}
				else if (entry.State == DbEntityEntryState.Updated)
				{
					EntityMutation<TEntity>.MutateEntity(entry.Entity, MutatorType.Update, Database);
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
				else if (entry.State == DbEntityEntryState.Deleted)
				{
					var idFieldValue = EntityMapper.GetIdValue(entry.Entity);
					var filter = Builders<TEntity>.Filter.Eq(idFieldName, idFieldValue);
					writeModel.Add(new DeleteOneModel<TEntity>(filter));
				}
			}

			return writeModel;
		}

		public void Write(IDbEntityCollection<TEntity> entityCollection)
		{
			var writeModel = BuildWriteModel(entityCollection);

			if (writeModel.Any())
			{
				//TODO: Add support for Transactions with MongoDB Server 4.0
				GetCollection().BulkWrite(writeModel);
			}
		}

		public async Task WriteAsync(IDbEntityCollection<TEntity> entityCollection, CancellationToken cancellationToken = default(CancellationToken))
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
