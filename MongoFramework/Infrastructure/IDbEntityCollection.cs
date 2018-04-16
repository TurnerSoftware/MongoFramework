using System.Collections.Generic;

namespace MongoFramework.Infrastructure
{
	public interface IDbEntityCollection<TEntity> : ICollection<TEntity>
	{
		DbEntityEntry<TEntity> GetEntry(TEntity entity);
		IEnumerable<DbEntityEntry<TEntity>> GetEntries();
		void Update(TEntity entity, DbEntityEntryState state);
	}
}
