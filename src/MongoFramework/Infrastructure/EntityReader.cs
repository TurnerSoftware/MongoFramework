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
		private IEntityDefinition EntityDefinition { get; }
		
		public EntityReader(IMongoDbConnection connection)
		{
			Connection = connection ?? throw new ArgumentNullException(nameof(connection));
			EntityDefinition = EntityMapping.GetOrCreateDefinition(typeof(TEntity));
		}

		private IMongoCollection<TEntity> GetCollection()
		{
			return Connection.GetDatabase().GetCollection<TEntity>(EntityDefinition.CollectionName);
		}

		public IQueryable<TEntity> AsQueryable()
		{
			var underlyingQueryable = GetCollection().AsQueryable();
			var queryable = new MongoFrameworkQueryable<TEntity, TEntity>(Connection, underlyingQueryable);
			queryable.EntityProcessors.Add(new EntityMutationProcessor<TEntity>());
			return queryable;
		}
	}
}
