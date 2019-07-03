using MongoDB.Driver;
using MongoFramework.Infrastructure.Linq;
using MongoFramework.Infrastructure.Linq.Processors;
using MongoFramework.Infrastructure.Mapping;
using System;
using System.Linq;

namespace MongoFramework.Infrastructure
{
	public class EntityReader<TEntity> : IEntityReader<TEntity> where TEntity : class
	{
		public IMongoDbConnection Connection { get; }
		
		public EntityReader(IMongoDbConnection connection)
		{
			Connection = connection ?? throw new ArgumentNullException(nameof(connection));
		}

		public IQueryable<TEntity> AsQueryable()
		{
			var provider = new MongoFrameworkQueryProvider<TEntity>(Connection);
			provider.EntityProcessors.Add(new EntityMutationProcessor<TEntity>());
			var queryable = new MongoFrameworkQueryable<TEntity>(provider);
			return queryable;
		}
	}
}
