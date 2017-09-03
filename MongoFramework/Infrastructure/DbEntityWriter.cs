using MongoDB.Bson;
using MongoDB.Driver;
using MongoFramework.Attributes;
using MongoFramework.Bson;
using MongoFramework.Infrastructure.Mutators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure
{
	public class DbEntityWriter<TEntity> : IDbEntityChangeWriter<TEntity>
	{
		public IMongoDatabase Database { get; set; }

		public IDbEntityMapper<TEntity> EntityMapper { get; private set; }
		
		public DbEntityWriter(IMongoDatabase database, IDbEntityMapper<TEntity> mapper)
		{
			Database = database;
			EntityMapper = mapper;
		}

		private IMongoCollection<TEntity> GetCollection()
		{
			var collectionName = EntityMapper.GetCollectionName();
			return Database.GetCollection<TEntity>(collectionName);
		}

		public void Add(TEntity entity)
		{
			AddRange(new[] { entity });
		}

		public void AddRange(IEnumerable<TEntity> entities)
		{
			DbEntityMutator<TEntity>.MutateEntities(entities, DbEntityMutatorType.Insert);
			GetCollection().InsertMany(entities);
		}
		
		public void Update(TEntity entity)
		{
			UpdateRange(new[] { entity });
		}

		public void UpdateRange(IEnumerable<TEntity> entities)
		{
			var idFieldName = EntityMapper.GetEntityIdName();
			var operations = new List<WriteModel<TEntity>>();

			DbEntityMutator<TEntity>.MutateEntities(entities, DbEntityMutatorType.Update);

			foreach (var entity in entities)
			{
				var idFieldValue = EntityMapper.GetEntityIdValue(entity);
				var filter = Builders<TEntity>.Filter.Eq(idFieldName, idFieldValue);
				var operation = new ReplaceOneModel<TEntity>(filter, entity);
				operations.Add(operation);
			}

			if (operations.Any())
			{
				GetCollection().BulkWrite(operations);
			}
		}

		public void Update(DbEntityEntry<TEntity> entry)
		{
			UpdateRange(new[] { entry });
		}

		public void UpdateRange(IEnumerable<DbEntityEntry<TEntity>> entries)
		{
			var idFieldName = EntityMapper.GetEntityIdName();
			var operations = new List<WriteModel<TEntity>>();

			DbEntityMutator<TEntity>.MutateEntities(entries.Select(e => e.Entity), DbEntityMutatorType.Update);

			foreach (var entry in entries)
			{
				if (entry.HasChanges())
				{
					var idFieldValue = EntityMapper.GetEntityIdValue(entry.Entity);
					var filter = Builders<TEntity>.Filter.Eq(idFieldName, idFieldValue);
					var updateDefintion = entry.GetUpdateDefinition();
					var operation = new UpdateOneModel<TEntity>(filter, updateDefintion);
					operations.Add(operation);
				}
			}

			if (operations.Any())
			{
				GetCollection().BulkWrite(operations);
			}
		}

		public void Remove(TEntity entity)
		{
			RemoveRange(new[] { entity });
		}

		public void RemoveRange(IEnumerable<TEntity> entities)
		{
			var idFieldName = EntityMapper.GetEntityIdName();
			FilterDefinition<TEntity> filter = null;
			
			foreach (var entity in entities)
			{
				var idFieldValue = EntityMapper.GetEntityIdValue(entity);

				if (filter != null)
				{
					filter = filter | Builders<TEntity>.Filter.Eq(idFieldName, idFieldValue);
				}
				else
				{
					filter = Builders<TEntity>.Filter.Eq(idFieldName, idFieldValue);
				}

				GetCollection().DeleteMany(filter);
			}
		}
	}
}
