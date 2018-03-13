using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace MongoFramework
{
	public interface IMongoDbSet
	{
		void SetDatabase(IMongoDatabase database);
		void SaveChanges();
	}

	public interface IAsyncMongoDbSet : IMongoDbSet
	{
		Task SaveChangesAsync();
	}

	public interface IMongoDbSet<TEntity> : IMongoDbSet, IQueryable<TEntity>
	{
		void Add(TEntity entity);
		void AddRange(IEnumerable<TEntity> entities);
		void Update(TEntity entity);
		void UpdateRange(IEnumerable<TEntity> entities);
		void Remove(TEntity entity);
		void RemoveRange(IEnumerable<TEntity> entities);
	}
}
