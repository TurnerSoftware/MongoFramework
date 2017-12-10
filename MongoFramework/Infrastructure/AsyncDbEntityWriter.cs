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
	public class AsyncDbEntityWriter<TEntity> : DbEntityWriter<TEntity>, IAsyncDbEntityChangeWriter<TEntity>
	{
		public AsyncDbEntityWriter(IMongoDatabase database) : this(database, new DbEntityMapper(typeof(TEntity))) { }

		public AsyncDbEntityWriter(IMongoDatabase database, IDbEntityMapper mapper) : base(database, mapper) { }

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
			DbEntityMutator<TEntity>.MutateEntities(entities, DbEntityMutatorType.Insert);
			await GetCollection().InsertManyAsync(entities);
		}

		public async Task UpdateAsync(TEntity entity)
		{
			await UpdateRangeAsync(new[] { entity });
		}

		public async Task UpdateRangeAsync(IEnumerable<TEntity> entities)
		{
			DbEntityMutator<TEntity>.MutateEntities(entities, DbEntityMutatorType.Update);
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
			DbEntityMutator<TEntity>.MutateEntities(entries.Select(e => e.Entity), DbEntityMutatorType.Update);
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
