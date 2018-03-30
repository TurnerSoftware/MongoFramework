using MongoDB.Driver;
using MongoFramework.Infrastructure.DefinitionHelpers;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mutation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure
{
	public class DbEntityWriter<TEntity> : IDbEntityWriter<TEntity>
	{
		public IMongoDatabase Database { get; set; }
		protected IEntityMapper EntityMapper { get; set; }

		public DbEntityWriter(IMongoDatabase database) : this(database, new EntityMapper(typeof(TEntity))) { }

		public DbEntityWriter(IMongoDatabase database, IEntityMapper mapper)
		{
			Database = database ?? throw new ArgumentNullException("database");
			EntityMapper = mapper ?? throw new ArgumentNullException("mapper");
		}

		private IMongoCollection<TEntity> GetCollection()
		{
			var collectionName = EntityMapper.GetCollectionName();
			return Database.GetCollection<TEntity>(collectionName);
		}

		private IEnumerable<WriteModel<TEntity>> BuildWriteModel(IDbEntityContainer<TEntity> entityContainer)
		{
			var idFieldName = EntityMapper.GetIdName();
			var writeModel = new List<WriteModel<TEntity>>();

			foreach (var entry in entityContainer.GetEntries())
			{
				if (entry.State == DbEntityEntryState.Added)
				{
					EntityMutation<TEntity>.MutateEntity(entry.Entity, MutatorType.Insert);
					writeModel.Add(new InsertOneModel<TEntity>(entry.Entity));
				}
				else if (entry.State == DbEntityEntryState.Updated)
				{
					EntityMutation<TEntity>.MutateEntity(entry.Entity, MutatorType.Update);
					var idFieldValue = EntityMapper.GetIdValue(entry.Entity);
					var filter = Builders<TEntity>.Filter.Eq(idFieldName, idFieldValue);
					var updateDefintion = UpdateDefinitionHelper.CreateFromDiff<TEntity>(entry.OriginalValues, entry.CurrentValues);
					writeModel.Add(new UpdateOneModel<TEntity>(filter, updateDefintion));
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

		public void Write(IDbEntityContainer<TEntity> entityContainer)
		{
			var writeModel = BuildWriteModel(entityContainer);
			//TODO: Add support for Transactions with MongoDB Server 4.0
			GetCollection().BulkWrite(writeModel);
		}

		public async Task WriteAsync(IDbEntityContainer<TEntity> entityContainer)
		{
			var writeModel = BuildWriteModel(entityContainer);
			//TODO: Add support for Transactions with MongoDB Server 4.0
			await GetCollection().BulkWriteAsync(writeModel);
		}
	}
}
