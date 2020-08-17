using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MongoFramework
{
	public interface IMongoDbSet
	{
		[Obsolete("Use SaveChanges on the IMongoDbContext")]
		void SaveChanges();
		[Obsolete("Use SaveChangesAsync on the IMongoDbContext")]
		Task SaveChangesAsync(CancellationToken cancellationToken = default);
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
