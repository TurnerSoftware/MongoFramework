using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MongoFramework
{
	public interface IMongoDbSet
	{
		void SetConnection(IMongoDbConnection connection);
		void SaveChanges();
		Task SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
	}

	public interface IMongoDbSet<TEntity> : IMongoDbSet, IQueryable<TEntity> where TEntity : class
	{
		TEntity Create();
		void Add(TEntity entity);
		void AddRange(IEnumerable<TEntity> entities);
		void Update(TEntity entity);
		void UpdateRange(IEnumerable<TEntity> entities);
		void Remove(TEntity entity);
		void RemoveRange(IEnumerable<TEntity> entities);
	}
}
