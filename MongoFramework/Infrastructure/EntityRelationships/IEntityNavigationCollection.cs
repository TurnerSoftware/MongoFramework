using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace MongoFramework.Infrastructure.EntityRelationships
{
	public interface IEntityNavigationCollection
	{
		void AddEntityById(object entityId);
		void AddEntitiesById(IEnumerable<object> entityIds);
		void LoadEntities();
		void Connect(IMongoDatabase database);
	}

	public interface IEntityNavigationCollection<TEntity> : IEntityNavigationCollection, IDbEntityCollection<TEntity>
	{

	}
}
