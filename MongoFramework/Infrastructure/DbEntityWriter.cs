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
		protected IDbEntityMapper EntityMapper { get; set; }

		public DbEntityWriter(IMongoDatabase database) : this(database, new DbEntityMapper(typeof(TEntity))) { }
		
		public DbEntityWriter(IMongoDatabase database, IDbEntityMapper mapper)
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

		protected IEnumerable<WriteModel<TEntity>> GenerateWriteOperations(IEnumerable<TEntity> entities)
		{
			var idFieldName = EntityMapper.GetIdName();
			var operations = new List<WriteModel<TEntity>>();

			foreach (var entity in entities)
			{
				var idFieldValue = EntityMapper.GetIdValue(entity);
				var filter = Builders<TEntity>.Filter.Eq(idFieldName, idFieldValue);
				var operation = new ReplaceOneModel<TEntity>(filter, entity);
				operations.Add(operation);
			}

			return operations;
		}

		public void Update(TEntity entity)
		{
			UpdateRange(new[] { entity });
		}

		public void UpdateRange(IEnumerable<TEntity> entities)
		{
			DbEntityMutator<TEntity>.MutateEntities(entities, DbEntityMutatorType.Update);
			var operations = GenerateWriteOperations(entities);

			if (operations.Any())
			{
				GetCollection().BulkWrite(operations);
			}
		}

		protected IEnumerable<WriteModel<TEntity>> GenerateWriteOperations(IEnumerable<DbEntityEntry<TEntity>> entries)
		{
			var idFieldName = EntityMapper.GetIdName();
			var operations = new List<WriteModel<TEntity>>();

			foreach (var entry in entries)
			{
				if (entry.HasChanges())
				{
					var idFieldValue = EntityMapper.GetIdValue(entry.Entity);
					var filter = Builders<TEntity>.Filter.Eq(idFieldName, idFieldValue);
					var updateDefintion = entry.GetUpdateDefinition();
					var operation = new UpdateOneModel<TEntity>(filter, updateDefintion);
					operations.Add(operation);
				}
			}

			return operations;
		}

		public void Update(DbEntityEntry<TEntity> entry)
		{
			UpdateRange(new[] { entry });
		}

		public void UpdateRange(IEnumerable<DbEntityEntry<TEntity>> entries)
		{
			DbEntityMutator<TEntity>.MutateEntities(entries.Select(e => e.Entity), DbEntityMutatorType.Update);
			var operations = GenerateWriteOperations(entries);

			if (operations.Any())
			{
				GetCollection().BulkWrite(operations);
			}
		}

		protected FilterDefinition<TEntity> GenerateIdFilter(IEnumerable<TEntity> entities)
		{
			var idFieldName = EntityMapper.GetIdName();
			FilterDefinition<TEntity> filter = null;

			foreach (var entity in entities)
			{
				var idFieldValue = EntityMapper.GetIdValue(entity);

				if (filter != null)
				{
					filter = filter | Builders<TEntity>.Filter.Eq(idFieldName, idFieldValue);
				}
				else
				{
					filter = Builders<TEntity>.Filter.Eq(idFieldName, idFieldValue);
				}
			}

			return filter;
		}

		public void Remove(TEntity entity)
		{
			RemoveRange(new[] { entity });
		}

		public void RemoveRange(IEnumerable<TEntity> entities)
		{
			var filter = GenerateIdFilter(entities);
			GetCollection().DeleteMany(filter);
		}
	}
}
