using MongoFramework.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure
{
	public class DbChangeTracker<TEntity> : IDbChangeTracker<TEntity>
	{
		private List<DbEntityEntry<TEntity>> Entries { get; set; } = new List<DbEntityEntry<TEntity>>();
		
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
		public void UpdateRange(IEnumerable<TEntity> entities, DbEntityEntryState state)
		{
			foreach (var entity in entities)
			{
				Update(entity, state);
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
		public void RemoveRange(IEnumerable<TEntity> entities)
		{
			foreach (var entity in entities)
			{
				Remove(entity);
			}
		}

		public void DetectChanges()
		{
			var entries = Entries.Where((DbEntityEntry<TEntity> e) => e.State == DbEntityEntryState.NoChanges || e.State == DbEntityEntryState.Updated);

			foreach (var entry in entries)
			{
				if (entry.HasChanges())
				{
					entry.State = DbEntityEntryState.Updated;
				}
				else
				{
					entry.State = DbEntityEntryState.NoChanges;
				}
			}
		}
		
		public void Clear()
		{
			Entries.Clear();
		}
	}
}