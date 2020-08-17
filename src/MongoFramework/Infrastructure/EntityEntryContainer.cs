using System;
using System.Collections.Generic;
using System.Linq;
using MongoFramework.Bson;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Infrastructure
{
	public class EntityEntryContainer
	{
		private Dictionary<Type, List<EntityEntry>> EntryLookupByType { get; } = new Dictionary<Type, List<EntityEntry>>();

		public IEnumerable<EntityEntry> Entries()
		{
			return EntryLookupByType.Values.SelectMany(e => e);
		}

		private void RemoveEntry<TCollectionBase>(EntityEntry entry)
		{
			if (EntryLookupByType.TryGetValue(typeof(TCollectionBase), out var entries))
			{
				entries.Remove(entry);
			}
		}

		private void AddEntry<TCollectionBase>(EntityEntry entry)
		{
			if (!EntryLookupByType.TryGetValue(typeof(TCollectionBase), out var entries))
			{
				entries = new List<EntityEntry>();
				EntryLookupByType.Add(typeof(TCollectionBase), entries);
			}

			entries.Add(entry);
		}

		public EntityEntry GetEntry<TCollectionBase>(TCollectionBase entity)
		{
			if (entity is null)
			{
				throw new ArgumentNullException(nameof(entity));
			}

			var collectionType = typeof(TCollectionBase);

			if (EntryLookupByType.TryGetValue(collectionType, out var entries))
			{
				var entityDefinition = EntityMapping.GetOrCreateDefinition(collectionType);
				var entityId = entityDefinition.GetIdValue(entity);
				var defaultIdValue = entityDefinition.GetDefaultId();
				foreach (var entry in entries)
				{
					if (Equals(entityId, defaultIdValue) && ReferenceEquals(entry.Entity, entity))
					{
						return entry;
					}
					else
					{
						var entryEntityId = entityDefinition.GetIdValue(entry.Entity);
						if (!Equals(entryEntityId, defaultIdValue) && entryEntityId.Equals(entityId))
						{
							return entry;
						}
					}
				}
			}

			return null;
		}

		public EntityEntry SetEntityState<TCollectionBase>(TCollectionBase entity, EntityEntryState state) where TCollectionBase : class
		{
			if (entity is null)
			{
				throw new ArgumentNullException(nameof(entity));
			}

			var entry = GetEntry(entity);
			if (entry != null)
			{
				if (ReferenceEquals(entry.Entity, entity))
				{
					if ((entry.State == EntityEntryState.Added && state == EntityEntryState.Deleted) || state == EntityEntryState.Detached)
					{
						entry.State = EntityEntryState.Detached;
						RemoveEntry<TCollectionBase>(entry);
					}
					else
					{
						entry.State = state;
					}
				}
				else if (state != EntityEntryState.Detached)
				{
					RemoveEntry<TCollectionBase>(entry);
					entry = new EntityEntry(entity, typeof(TCollectionBase), state);
					AddEntry<TCollectionBase>(entry);
				}
			}
			else if (state != EntityEntryState.Detached)
			{
				entry = new EntityEntry(entity, typeof(TCollectionBase), state);
				AddEntry<TCollectionBase>(entry);
			}

			return entry;
		}

		/// <summary>
		/// Performed after saving, this resets the state of entries to align with the state in the database.
		/// </summary>
		public void CommitChanges()
		{
			foreach (var typeGroup in EntryLookupByType.Values)
			{
				for (var i = typeGroup.Count - 1; i >= 0; i--)
				{
					var entry = typeGroup[i];
					if (entry.State == EntityEntryState.Added || entry.State == EntityEntryState.Updated)
					{
						entry.ResetState();
					}
					else if (entry.State == EntityEntryState.Deleted)
					{
						typeGroup.Remove(entry);
					}
				}
			}
		}

		/// <summary>
		/// Performed prior to saving, this detect changes between the original and current state of entries, updating states where necessary.
		/// </summary>
		public void DetectChanges()
		{
			foreach (var typeGroup in EntryLookupByType.Values)
			{
				for (var i = typeGroup.Count - 1; i >= 0; i--)
				{
					var entry = typeGroup[i];
					if (entry.State == EntityEntryState.NoChanges || entry.State == EntityEntryState.Updated)
					{
						if (BsonDiff.HasDifferences(entry.OriginalValues, entry.CurrentValues))
						{
							entry.State = EntityEntryState.Updated;
						}
						else
						{
							entry.State = EntityEntryState.NoChanges;
						}
					}
				}
			}
		}

		public void Clear()
		{
			EntryLookupByType.Clear();
		}
	}
}
