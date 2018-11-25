using System.Collections.Generic;

namespace MongoFramework.Infrastructure
{
	public interface IEntityCollection<TEntity> : ICollection<TEntity> where TEntity : class
	{
		EntityEntry<TEntity> GetEntry(TEntity entity);
		IEnumerable<EntityEntry<TEntity>> GetEntries();
		void Update(TEntity entity, EntityEntryState state);
	}
}
