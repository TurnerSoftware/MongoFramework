using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using MongoDB.Driver.Linq;
using MongoFramework.Infrastructure.Linq;
using MongoFramework.Infrastructure.Linq.Processors;

namespace MongoFramework.Infrastructure
{
	public class DbEntityReader<TEntity> : IDbEntityReader<TEntity>
	{
		public IMongoDatabase Database { get; set; }
		public IDbEntityMapper<TEntity> EntityMapper { get; private set; }

		public DbEntityReader(IMongoDatabase database, IDbEntityMapper<TEntity> mapper)
		{
			Database = database;
			EntityMapper = mapper;
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
