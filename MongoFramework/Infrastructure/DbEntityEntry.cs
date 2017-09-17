using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoFramework.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure
{
	public class DbEntityEntry<TEntity>
	{
		/// <summary>
		/// The state of the entity in this <see cref="DbEntityEntry{TEntity}"/> object.
		/// </summary>
		public DbEntityEntryState State { get; set; }

		/// <summary>
		/// The original values of the entity.
		/// </summary>
		public BsonDocument OriginalValues { get; private set; }

		/// <summary>
		/// The entity that forms this <see cref="DbEntityEntry{TEntity}"/> object.
		/// </summary>
		public TEntity Entity { get; private set; }

		/// <summary>
		/// Creates a new <see cref="DbEntityEntry{TEntity}"/> with the specified entity and state information.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="state"></param>
		public DbEntityEntry(TEntity entity, DbEntityEntryState state)
		{
			State = state;
			Entity = entity;

			if (state == DbEntityEntryState.NoChanges)
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

			if (State == DbEntityEntryState.Updated || State == DbEntityEntryState.NoChanges)
			{
				if (this.HasChanges())
				{
					State = DbEntityEntryState.Updated;
				}
				else
				{
					State = DbEntityEntryState.NoChanges;
				}
			}
		}

		/// <summary>
		/// The current values of the entity.
		/// </summary>
		public BsonDocument CurrentValues
		{
			get
			{
				return Entity.ToBsonDocument();
			}
		}
	}
}
