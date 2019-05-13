using MongoFramework.Infrastructure.Mapping;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MongoFramework.Infrastructure
{
	public class EntityCollection<TEntity> : IEntityCollection<TEntity> where TEntity : class
	{
		protected List<EntityEntry<TEntity>> Entries { get; } = new List<EntityEntry<TEntity>>();

		private IEntityDefinition EntityDefinition { get; }

		public EntityCollection()
		{
			EntityDefinition = EntityMapping.GetOrCreateDefinition(typeof(TEntity));
		}

		public int Count => Entries.Count;

		public bool IsReadOnly => false;

		public EntityEntry<TEntity> GetEntry(TEntity entity)
		{
			var entityId = EntityDefinition.GetIdValue(entity);
			var defaultIdValue = EntityDefinition.GetDefaultId();

			foreach (var entry in Entries)
			{
				if (Equals(entityId, defaultIdValue) && entry.Entity.Equals(entity))
				{
					return entry;
				}
				else
				{
					var entryEntityId = EntityDefinition.GetIdValue(entry.Entity);
					if (!Equals(entryEntityId, defaultIdValue) && entryEntityId.Equals(entityId))
					{
						return entry;
					}
				}
			}

			return null;
		}

		public IEnumerable<EntityEntry<TEntity>> GetEntries()
		{
			return Entries;
		}

		public void Update(TEntity entity, EntityEntryState state)
		{
			var entry = GetEntry(entity);
			if (entry != null)
			{
				if (entry.Entity.Equals(entity))
				{
					entry.State = state;
				}
				else
				{
					Entries.Remove(entry);
					Entries.Add(new EntityEntry<TEntity>(entity, state));
				}
			}
			else
			{
				Entries.Add(new EntityEntry<TEntity>(entity, state));
			}
		}

		public bool Remove(TEntity entity)
		{
			var entry = GetEntry(entity);
			if (entry != null)
			{
				Entries.Remove(entry);
				return true;
			}
			return false;
		}

		public void Clear()
		{
			Entries.Clear();
		}

		public void Add(TEntity item)
		{
			var defaultId = EntityDefinition.GetDefaultId();
			var entityId = EntityDefinition.GetIdValue(item);
			if (Equals(entityId, defaultId))
			{
				Update(item, EntityEntryState.Added);
			}
			else
			{
				Update(item, EntityEntryState.NoChanges);
			}
		}

		public bool Contains(TEntity item)
		{
			return GetEntry(item) != null;
		}

		public void CopyTo(TEntity[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException(nameof(array));
			}

			if (arrayIndex < 0 || array.Length - arrayIndex < Count)
			{
				throw new IndexOutOfRangeException();
			}

			for (var i = 0; i < Count; i++)
			{
				array[i + arrayIndex] = Entries[i].Entity;
			}
		}

		public IEnumerator<TEntity> GetEnumerator()
		{
			var result = GetEntries().Select(e => e.Entity);
			using (var enumerator = result.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					yield return enumerator.Current;
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
