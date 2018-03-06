using MongoDB.Bson;
using MongoDB.Driver;
using MongoFramework.Attributes;
using MongoFramework.Bson;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mutation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure
{
	public class AsyncDbEntityWriter<TEntity> : DbEntityWriter<TEntity>, IAsyncDbEntityChangeWriter<TEntity>
	{
		public AsyncDbEntityWriter(IMongoDatabase database) : this(database, new EntityMapper(typeof(TEntity))) { }

		public AsyncDbEntityWriter(IMongoDatabase database, IEntityMapper mapper) : base(database, mapper) { }

		private IMongoCollection<TEntity> GetCollection()
		{
			var collectionName = EntityMapper.GetCollectionName();
			return Database.GetCollection<TEntity>(collectionName);
		}

		public async Task AddAsync(TEntity entity)
		{
			await AddRangeAsync(new[] { entity });
		}

		public async Task AddRangeAsync(IEnumerable<TEntity> entities)
		{
			EntityMutation<TEntity>.MutateEntities(entities, MutatorType.Insert);
			await GetCollection().InsertManyAsync(entities);
		}

		public async Task UpdateAsync(TEntity entity)
		{
			await UpdateRangeAsync(new[] { entity });
		}

		public async Task UpdateRangeAsync(IEnumerable<TEntity> entities)
		{
			EntityMutation<TEntity>.MutateEntities(entities, MutatorType.Update);
			var operations = GenerateWriteOperations(entities);

			if (operations.Any())
			{
				await GetCollection().BulkWriteAsync(operations);
			}
		}

		public async Task UpdateAsync(DbEntityEntry<TEntity> entry)
		{
			await UpdateRangeAsync(new[] { entry });
		}

		public async Task UpdateRangeAsync(IEnumerable<DbEntityEntry<TEntity>> entries)
		{
			EntityMutation<TEntity>.MutateEntities(entries.Select(e => e.Entity), MutatorType.Update);
			var operations = GenerateWriteOperations(entries);

			if (operations.Any())
			{
				await GetCollection().BulkWriteAsync(operations);
			}
		}

		public async Task RemoveAsync(TEntity entity)
		{
			await RemoveRangeAsync(new[] { entity });
		}

		public async Task RemoveRangeAsync(IEnumerable<TEntity> entities)
		{
			var filter = GenerateIdFilter(entities);
			await GetCollection().DeleteManyAsync(filter);
		}
	}
}
