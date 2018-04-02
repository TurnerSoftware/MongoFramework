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

		public DbEntityReader(IMongoDatabase database) : this(database, new EntityMapper(typeof(TEntity))) { }

		public DbEntityReader(IMongoDatabase database, IEntityMapper mapper)
		{
			Database = database ?? throw new ArgumentNullException(nameof(database));
			EntityMapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
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
			queryable.EntityProcessors.Add(new EntityMutationProcessor<TEntity>());
			return queryable;
		}
	}
}
