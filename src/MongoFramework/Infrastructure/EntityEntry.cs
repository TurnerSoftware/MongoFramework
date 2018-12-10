using MongoDB.Bson;

namespace MongoFramework.Infrastructure
{
	public class EntityEntry<TEntity> where TEntity : class
	{
		/// <summary>
		/// The state of the entity in this <see cref="EntityEntry{TEntity}"/> object.
		/// </summary>
		public EntityEntryState State { get; set; }

		/// <summary>
		/// The original values of the entity.
		/// </summary>
		public BsonDocument OriginalValues { get; private set; }

		/// <summary>
		/// The entity that forms this <see cref="EntityEntry{TEntity}"/> object.
		/// </summary>
		public TEntity Entity { get; private set; }

		/// <summary>
		/// Creates a new <see cref="EntityEntry{TEntity}"/> with the specified entity and state information.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="state"></param>
		public EntityEntry(TEntity entity, EntityEntryState state)
		{
			State = state;
			Entity = entity;

			if (state == EntityEntryState.NoChanges)
			{
				Refresh();
			}
		}

		/// <summary>
		/// Update the original values to reflect the current state of the entity.
		/// </summary>
		public void Refresh()
		{
			Refresh(Entity);
		}

		/// <summary>
		/// Update the original values to reflect the state of the provided entity.
		/// </summary>
		/// <param name="entity"></param>
		public void Refresh(TEntity entity)
		{
			OriginalValues = entity.ToBsonDocument();
			State = this.HasChanges() ? EntityEntryState.Updated : EntityEntryState.NoChanges;
		}

		/// <summary>
		/// The current values of the entity.
		/// </summary>
		public BsonDocument CurrentValues => Entity.ToBsonDocument();
	}
}
