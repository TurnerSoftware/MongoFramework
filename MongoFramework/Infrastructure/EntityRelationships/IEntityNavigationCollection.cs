using MongoDB.Driver;
using System.Collections.Generic;

namespace MongoFramework.Infrastructure.EntityRelationships
{
	public interface IEntityNavigationCollection
	{
		void AddForeignId(object foreignId);
		void AddForeignIds(IEnumerable<object> foreignIds);
		void LoadEntities();
	}

	public interface IEntityNavigationCollection<TEntity> : IEntityNavigationCollection, IEntityCollection<TEntity> where TEntity : class
	{

	}
}
