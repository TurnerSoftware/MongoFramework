using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MongoFramework.Infrastructure
{
	public class EntityNavigationCollection<TEntity> : EntityCollection<TEntity>, IEntityNavigationCollection<TEntity> where TEntity : class
	{
		private IMongoDbConnection Connection { get; set; }
		private HashSet<object> UnloadedIds { get; } = new HashSet<object>();

		public IEntityProperty ForeignProperty { get; }

		public new int Count => LoadedCount + UnloadedCount;

		public int LoadedCount => Entries.Count;
		public int UnloadedCount => UnloadedIds.Count;
		
		public EntityNavigationCollection(IEntityProperty foreignProperty) : base()
		{
			ForeignProperty = foreignProperty ?? throw new ArgumentNullException(nameof(foreignProperty));
		}

		public void SetConnection(IMongoDbConnection connection)
		{
			Connection = connection;
		}
		
		public void AddForeignId(object foreignId)
		{
			if (foreignId == null)
			{
				throw new ArgumentNullException(nameof(foreignId));
			}

			//Check the EntityId matches the known type for TEntity
			if (!ForeignProperty.PropertyType.Equals(foreignId.GetType()))
			{
				throw new InvalidOperationException($"Type mismatch for foreign key. {foreignId.GetType()} specified however expected type {ForeignProperty.PropertyType}");
			}

			//Check the entity isn't already loaded
			if (!Entries.Any(e => Equals(foreignId, ForeignProperty.GetValue(e.Entity))))
			{
				UnloadedIds.Add(foreignId);
			}
		}

		public void AddForeignIds(IEnumerable<object> foreignIds)
		{
			foreach (var foreignId in foreignIds)
			{
				AddForeignId(foreignId);
			}
		}

		public void LoadEntities()
		{
			if (!UnloadedIds.Any())
			{
				return;
			}

			var dbEntityReader = new EntityReader<TEntity>(Connection);
			var entities = dbEntityReader.AsQueryable().WherePropertyMatches(ForeignProperty, UnloadedIds);

			foreach (var entity in entities)
			{
				Update(entity, EntityEntryState.NoChanges);
			}

			UnloadedIds.Clear();
		}

		public IEnumerable<object> GetForeignIds()
		{
			var loadedEntityIds = Entries.Select(e => ForeignProperty.GetValue(e.Entity));
			return loadedEntityIds.Concat(UnloadedIds);
		}

		public new IEnumerator<TEntity> GetEnumerator()
		{
			//Enumerate loaded entities
			var result = Entries.Select(e => e.Entity);
			using (var enumerator = result.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					yield return enumerator.Current as TEntity;
				}
			}

			//Enumerate list of unloaded IDs and load them in one at a time
			if (UnloadedIds.Any())
			{
				var dbEntityReader = new EntityReader<TEntity>(Connection);
				var unloadedEntities = dbEntityReader.AsQueryable().WherePropertyMatches(ForeignProperty, UnloadedIds);

				using (var unloadedEnumerator = unloadedEntities.GetEnumerator())
				{
					while (unloadedEnumerator.MoveNext())
					{
						var loadedEntity = unloadedEnumerator.Current;

						//Load the data into the collection so we don't need to query it again
						Update(loadedEntity, EntityEntryState.NoChanges);

						//Remove from unloaded entity collection
						var foreignId = ForeignProperty.GetValue(loadedEntity);
						UnloadedIds.Remove(foreignId);

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
