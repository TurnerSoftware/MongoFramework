using MongoDB.Driver;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MongoFramework.Infrastructure.EntityRelationships
{
	public class EntityNavigationCollection<TEntity> : DbEntityCollection<TEntity>, IEntityNavigationCollection<TEntity>
	{
		private IMongoDatabase Database { get; set; }
		private IEntityMapper EntityMapper { get; } = new EntityMapper<TEntity>();
		private HashSet<object> UnloadedEntityIds { get; } = new HashSet<object>();

		public new int Count => LoadedCount + UnloadedCount;

		public int LoadedCount => Entries.Count;
		public int UnloadedCount => UnloadedEntityIds.Count;

		public void Connect(IMongoDatabase database)
		{
			Database = database;
		}

		public void AddEntityById(object entityId)
		{
			if (entityId == null)
			{
				throw new ArgumentNullException(nameof(entityId));
			}

			//Check the EntityId matches the known type for TEntity
			var idPropertyType = EntityMapper.GetEntityMapping().Where(m => m.IsKey).Select(m => m.PropertyType).FirstOrDefault();
			if (!idPropertyType.Equals(entityId.GetType()))
			{
				throw new InvalidOperationException($"Invalid entity ID type for {nameof(TEntity)}");
			}
			
			//Check the entity isn't already loaded
			if (!Entries.Any(e => Equals(entityId, EntityMapper.GetIdValue(e.Entity))))
			{
				UnloadedEntityIds.Add(entityId);
			}
		}

		public void AddEntitiesById(IEnumerable<object> entityIds)
		{
			foreach (var entityId in entityIds)
			{
				AddEntityById(entityId);
			}
		}

		public void LoadEntities()
		{
			if (!UnloadedEntityIds.Any())
			{
				return;
			}

			var dbEntityReader = new DbEntityReader<TEntity>(Database);
			var entities = dbEntityReader.AsQueryable().WhereIdMatches(UnloadedEntityIds);

			foreach (var entity in entities)
			{
				Update(entity, DbEntityEntryState.NoChanges);
			}

			UnloadedEntityIds.Clear();
		}

		public IEnumerable<object> GetEntityIds()
		{
			var loadedEntityIds = Entries.Select(e => EntityMapper.GetIdValue(e.Entity));
			return UnloadedEntityIds.Concat(loadedEntityIds);
		}

		public new IEnumerator<TEntity> GetEnumerator()
		{
			//Enumerate loaded entities
			var result = Entries.Select(e => e.Entity);
			using (var enumerator = result.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					yield return enumerator.Current;
				}
			}

			//Enumerate list of unloaded entity IDs and load them in one at a time
			if (UnloadedEntityIds.Any())
			{
				var entityMapper = new EntityMapper<TEntity>();
				var dbEntityReader = new DbEntityReader<TEntity>(Database);
				var unloadedEntities = dbEntityReader.AsQueryable().WhereIdMatches(UnloadedEntityIds);

				using (var unloadedEnumerator = unloadedEntities.GetEnumerator())
				{
					while (unloadedEnumerator.MoveNext())
					{
						var loadedEntity = unloadedEnumerator.Current;

						//Load the data into the collection so we don't need to query it again
						Update(loadedEntity, DbEntityEntryState.NoChanges);

						//Remove from unloaded entity collection
						var entityId = entityMapper.GetIdValue(loadedEntity);
						UnloadedEntityIds.Remove(entityId);

						yield return loadedEntity;
					}
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
