using MongoDB.Bson;

namespace MongoFramework.Infrastructure
{
	public class EntityEntry
	{
		/// <summary>
		/// The state of the entity in this <see cref="EntityEntry"/> object.
		/// </summary>
		public EntityEntryState State { get; set; }

		/// <summary>
		/// The original values of the entity.
		/// </summary>
		public BsonDocument OriginalValues { get; private set; }

		/// <summary>
		/// The entity that forms this <see cref="EntityEntry{TEntity}"/> object.
		/// </summary>
		public object Entity { get; private set; }

		/// <summary>
		/// Creates a new <see cref="EntityEntry"/> with the specified entity and state information.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="state"></param>
		public EntityEntry(object entity, EntityEntryState state)
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
		public void Refresh(object entity)
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
