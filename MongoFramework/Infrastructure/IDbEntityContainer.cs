using System;
using System.Collections.Generic;

namespace MongoFramework.Infrastructure
{
	public interface IDbEntityContainer<TEntity>
	{
		DbEntityEntry<TEntity> GetEntry(TEntity entity);
		IEnumerable<DbEntityEntry<TEntity>> GetEntries();
		void Update(TEntity entity, DbEntityEntryState state);
		void Remove(TEntity entity);
		void Clear();
	}
}
