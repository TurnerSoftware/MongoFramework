using MongoDB.Driver;
using MongoFramework.Infrastructure.Linq;
using MongoFramework.Infrastructure.Linq.Processors;
using MongoFramework.Infrastructure.Mapping;
using System;
using System.Linq;

namespace MongoFramework.Infrastructure
{
	public class DbEntityReader<TEntity> : IDbEntityReader<TEntity>
	{
		public IMongoDatabase Database { get; private set; }
		public IEntityMapper EntityMapper { get; private set; }

		public DbEntityReader(IDbContextSettings settings)
		{
			Database = settings.GetDatabase();
			EntityMapper = settings.GetEntityMapper<TEntity>();
		}

		private IMongoCollection<TEntity> GetCollection()
		{
			var collectionName = EntityMapper.GetCollectionName();
			return Database.GetCollection<TEntity>(collectionName);
		}

		public IQueryable<TEntity> AsQueryable()
		{
			var underlyingQueryable = GetCollection().AsQueryable();
			var queryable = new MongoFrameworkQueryable<TEntity, TEntity>(underlyingQueryable);
			queryable.EntityProcessors.Add(new EntityMutationProcessor<TEntity>(Database));
			return queryable;
		}
	}
}
