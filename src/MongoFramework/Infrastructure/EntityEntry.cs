using System;
using MongoDB.Bson;

namespace MongoFramework.Infrastructure
{
	public class EntityEntry
	{
		/// <summary>
		/// The entity that forms this <see cref="EntityEntry"/> object.
		/// </summary>
		public object Entity { get; }

		public Type EntityType { get; }

		/// <summary>
		/// The state of the entity in this <see cref="EntityEntry"/> object.
		/// </summary>
		public EntityEntryState State { get; internal set; }

		/// <summary>
		/// The original values of the entity.
		/// </summary>
		public BsonDocument OriginalValues { get; private set; }

		/// <summary>
		/// Creates a new <see cref="EntityEntry"/> with the specified entity and state information.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="state"></param>
		public EntityEntry(object entity, EntityEntryState state)
		{
			State = state;
			Entity = entity;
			EntityType = entity.GetType();

			if (state == EntityEntryState.NoChanges)
			{
				OriginalValues = Entity.ToBsonDocument();
			}
		}

		/// <summary>
		/// Creates a new <see cref="EntityEntry"/> with the specified entity, type and state information.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="entityType"></param>
		/// <param name="state"></param>
		public EntityEntry(object entity, Type entityType, EntityEntryState state) : this(entity, state)
		{
			EntityType = entityType;
		}

		/// <summary>
		/// Resets the state of the entry, ready for tracking new changes.
		/// </summary>
		public void ResetState()
		{
			OriginalValues = Entity.ToBsonDocument();
			State = EntityEntryState.NoChanges;
		}

		/// <summary>
		/// The current values of the entity.
		/// </summary>
		public BsonDocument CurrentValues => Entity.ToBsonDocument();
	}
}
