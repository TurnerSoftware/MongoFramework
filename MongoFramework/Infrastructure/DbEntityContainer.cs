using MongoFramework.Infrastructure.Mapping;
using System.Collections.Generic;

namespace MongoFramework.Infrastructure
{
	public class DbEntityContainer<TEntity> : IDbEntityContainer<TEntity>
	{
		protected List<DbEntityEntry<TEntity>> Entries { get; } = new List<DbEntityEntry<TEntity>>();

		public DbEntityEntry<TEntity> GetEntry(TEntity entity)
		{
			var entityMapper = new EntityMapper<TEntity>();
			var entityId = entityMapper.GetIdValue(entity);

			foreach (var entry in Entries)
			{
				if (entityId == null && entry.Entity.Equals(entity))
				{
					return entry;
				}
				else
				{
					var entryEntityId = entityMapper.GetIdValue(entry.Entity);
					if (entryEntityId != null && entryEntityId.Equals(entityId))
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

		public void Remove(TEntity entity)
		{
			var entry = GetEntry(entity);
			if (entry != null)
			{
				Entries.Remove(entry);
			}
		}

		public void Clear()
		{
			Entries.Clear();
		}
	}
}
