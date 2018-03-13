﻿using System.Linq;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Linq;
using MongoFramework.Infrastructure.Linq.Processors;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Infrastructure
{
	public class DbEntityReader<TEntity> : IDbEntityReader<TEntity>
	{
		public IMongoDatabase Database { get; set; }
		private IEntityMapper EntityMapper { get; set; }

		public DbEntityReader(IMongoDatabase database) : this(database, new EntityMapper(typeof(TEntity))) { }

		public DbEntityReader(IMongoDatabase database, IEntityMapper mapper)
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
