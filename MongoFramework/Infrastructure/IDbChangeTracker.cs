using System.Collections.Generic;

namespace MongoFramework.Infrastructure
{
	public interface IDbChangeTracker<TEntity>
	{
		DbEntityEntry<TEntity> GetEntry(TEntity entity);
		IEnumerable<DbEntityEntry<TEntity>> GetEntries();
		void Update(TEntity entity, DbEntityEntryState state);
		void UpdateRange(IEnumerable<TEntity> entities, DbEntityEntryState state);
		void Remove(TEntity entity);
		void RemoveRange(IEnumerable<TEntity> entities);
		void DetectChanges();
		void Clear();
	}
}
