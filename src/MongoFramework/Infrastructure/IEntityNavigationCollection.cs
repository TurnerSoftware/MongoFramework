using System.Collections.Generic;

namespace MongoFramework.Infrastructure
{
	public interface IEntityNavigationCollectionBase
	{
		void SetConnection(IMongoDbConnection connection);
		void AddForeignId(object foreignId);
		void AddForeignIds(IEnumerable<object> foreignIds);
		void LoadEntities();
	}

	public interface IEntityNavigationCollection<TEntity> : IEntityNavigationCollectionBase, IEntityCollection<TEntity> where TEntity : class
	{

	}
}
