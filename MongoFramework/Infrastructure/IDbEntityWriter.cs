using System.Collections.Generic;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure
{
	public interface IDbEntityWriter<TEntity>
	{
		void Add(TEntity entity);
		void AddRange(IEnumerable<TEntity> entities);
		void Update(TEntity entity);
		void UpdateRange(IEnumerable<TEntity> entities);
		void Remove(TEntity entity);
		void RemoveRange(IEnumerable<TEntity> entities);
	}

	public interface IAsyncDbEntityWriter<TEntity> : IDbEntityWriter<TEntity>
	{
		Task AddAsync(TEntity entity);
		Task AddRangeAsync(IEnumerable<TEntity> entities);
		Task UpdateAsync(TEntity entity);
		Task UpdateRangeAsync(IEnumerable<TEntity> entities);
		Task RemoveAsync(TEntity entity);
		Task RemoveRangeAsync(IEnumerable<TEntity> entities);
	}
}
