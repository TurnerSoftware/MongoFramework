using System.Collections.Generic;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure
{
	public interface IDbEntityChangeWriter<TEntity> : IDbEntityWriter<TEntity>
	{
		void Update(DbEntityEntry<TEntity> entry);
		void UpdateRange(IEnumerable<DbEntityEntry<TEntity>> entries);
	}

	public interface IAsyncDbEntityChangeWriter<TEntity> : IAsyncDbEntityWriter<TEntity>, IDbEntityChangeWriter<TEntity>
	{
		Task UpdateAsync(DbEntityEntry<TEntity> entry);
		Task UpdateRangeAsync(IEnumerable<DbEntityEntry<TEntity>> entries);
	}
}
