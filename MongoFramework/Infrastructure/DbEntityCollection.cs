using MongoFramework.Infrastructure.Mapping;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MongoFramework.Infrastructure
{
	public class DbEntityCollection<TEntity> : IDbEntityCollection<TEntity> where TEntity : class
	{
		protected List<DbEntityEntry<TEntity>> Entries { get; } = new List<DbEntityEntry<TEntity>>();

		private EntityMapper<TEntity> EntityMapper { get; } = new EntityMapper<TEntity>();

		public int Count => Entries.Count;

		public bool IsReadOnly => false;

		public DbEntityEntry<TEntity> GetEntry(TEntity entity)
		{
			var entityId = EntityMapper.GetIdValue(entity);
			var defaultIdValue = EntityMapper.GetDefaultId();

			foreach (var entry in Entries)
			{
				if (Equals(entityId, defaultIdValue) && entry.Entity.Equals(entity))
				{
					return entry;
				}
				else
				{
					var entryEntityId = EntityMapper.GetIdValue(entry.Entity);
					if (!Equals(entryEntityId, defaultIdValue) && entryEntityId.Equals(entityId))
					{
						return entry;
					}
				}
			}

			return null;
		}

		public IEnumerable<DbEntityEntry<TEntity>> GetEntries()
		{
			return Entries;
		}

		public void Update(TEntity entity, DbEntityEntryState state)
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
					Entries.Add(new DbEntityEntry<TEntity>(entity, state));
				}
			}
			else
			{
				Entries.Add(new DbEntityEntry<TEntity>(entity, state));
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
			//TODO: Check the ID value is a default value - if not, mark it as non-changed
			Update(item, DbEntityEntryState.Added);
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
			var result = Entries.Select(e => e.Entity);
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
