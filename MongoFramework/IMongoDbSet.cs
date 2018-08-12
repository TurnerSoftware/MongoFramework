using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MongoFramework
{
	public interface IMongoDbSet
	{
		void SaveChanges();
		Task SaveChangesAsync(CancellationToken cancellationToken = default);
	}

	public interface IMongoDbSet<TEntity> : IMongoDbSet, IQueryable<TEntity>
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
