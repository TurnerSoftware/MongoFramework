using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MongoFramework.Core
{
	public class EntityChangeSet<TEntity>
	{
		private IDictionary<TEntity, EntityState> objectStates { get; set; }
		
		public EntityChangeSet()
		{
			objectStates = new Dictionary<TEntity, EntityState>();
		}
		
		public ReadOnlyDictionary<TEntity, EntityState> GetAllChanges()
		{
			return new ReadOnlyDictionary<TEntity, EntityState>(objectStates);
		}

		public void ClearChanges()
		{
			objectStates.Clear();
		}
		
		public EntityState GetEntityState(TEntity entity)
		{
			if (objectStates.ContainsKey(entity))
			{
				return objectStates[entity];
			}

			return EntityState.NoChanges;
		}

		public IEnumerable<TEntity> GetEntitiesByEntityState(EntityState state)
		{
			return objectStates.Where(kvp => kvp.Value == state).Select(kvp => kvp.Key);
		}

		public void SetEntityState(TEntity entity, EntityState state)
		{
			if (state == EntityState.NoChanges)
			{
				if (objectStates.ContainsKey(entity))
				{
					objectStates.Remove(entity);
				}

				return;
			}

			if (objectStates.ContainsKey(entity))
			{
				objectStates[entity] = state;
			}
			else
			{
				objectStates.Add(entity, state);
			}
		}
	}
}
