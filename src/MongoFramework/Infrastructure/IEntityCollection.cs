using System.Collections.Generic;

namespace MongoFramework.Infrastructure
{
	public interface IEntityCollection<TEntity> : ICollection<TEntity> where TEntity : class
	{
		EntityEntry GetEntry(TEntity entity);
		IEnumerable<EntityEntry> GetEntries();
		void Update(TEntity entity, EntityEntryState state);
	}
}
