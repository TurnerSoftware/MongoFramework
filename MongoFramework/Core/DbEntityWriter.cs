using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Core
{
	public class DbEntityWriter<TEntity> : IDbEntityWriter<TEntity>
	{
		public IMongoDatabase Database { get; set; }
		public string CollectionName { get; private set; }

		private IDbEntityWorkflow<TEntity> entityWorkflow { get; set; }

		public DbEntityWriter(IMongoDatabase database) : this(database, null) { }

		public DbEntityWriter(IMongoDatabase database, string collectionName)
		{
			Database = database;

			entityWorkflow = new DbEntityWorkflow<TEntity>();
			entityWorkflow.ConfigureEntity();

			if (!string.IsNullOrEmpty(collectionName))
			{
				CollectionName = collectionName;
			}
			else
			{
				CollectionName = entityWorkflow.GetCollectionName();
			}
		}
		
		private IMongoCollection<TEntity> GetCollection()
		{
			return Database.GetCollection<TEntity>(CollectionName);
		}

		public void InsertEntity(TEntity entity)
		{
			GetCollection().InsertOne(entity);
		}

		public void InsertEntities(IEnumerable<TEntity> entities)
		{
			GetCollection().InsertMany(entities);
		}

		public void UpdateEntity(TEntity entity)
		{
			var idFieldName = entityWorkflow.GetEntityIdName();
			var idFieldValue = entityWorkflow.GetEntityIdValue(entity);
			var filter = Builders<TEntity>.Filter.Eq(idFieldName, idFieldValue);

			GetCollection().ReplaceOne(filter, entity);
		}

		public void UpdateEntities(IEnumerable<TEntity> entities)
		{
			foreach (var entity in entities)
			{
				UpdateEntity(entity);
			}
		}

		public void DeleteEntity(TEntity entity)
		{
			var idFieldName = entityWorkflow.GetEntityIdName();
			var idFieldValue = entityWorkflow.GetEntityIdValue(entity);
			var filter = Builders<TEntity>.Filter.Eq(idFieldName, idFieldValue);

			GetCollection().DeleteOne(filter);
		}

		public void DeleteEntities(IEnumerable<TEntity> entities)
		{
			//TODO: Dynamically build filter and do a single call to DeleteMany
			foreach (var entity in entities)
			{
				DeleteEntity(entity);
			}
		}

		public void DeleteMatching(Expression<Func<TEntity, bool>> criteria)
		{
			GetCollection().DeleteMany(criteria);
		}

		public void DeleteById(object id)
		{
			var idFieldName = entityWorkflow.GetEntityIdName();
			var filter = Builders<TEntity>.Filter.Eq(idFieldName, id);

			GetCollection().DeleteOne(filter);
		}
	}
}
