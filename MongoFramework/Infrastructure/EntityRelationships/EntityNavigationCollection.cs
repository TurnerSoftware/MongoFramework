using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Linq;

namespace MongoFramework.Infrastructure.EntityRelationships
{
	public class EntityNavigationCollection<TEntity> : DbEntityChangeTracker<TEntity>, IEntityNavigationCollection<TEntity>
	{
		private IEnumerable<object> UnloadedEntityIds { get; set; } = Enumerable.Empty<object>();

		public IEnumerable<object> PersistingEntityIds
		{
			get
			{
				var entityMapper = new EntityMapper<TEntity>();
				var loadedEntityIds = Entries.Where(e => e.State != DbEntityEntryState.Deleted).Select(e => entityMapper.GetIdValue(e.Entity));
				return loadedEntityIds.Concat(UnloadedEntityIds);
			}
		}

		public void BeginImport(IEnumerable<object> entityIds)
		{
			UnloadedEntityIds = entityIds;
		}

		public void FinaliseImport(IMongoDatabase database)
		{
			if (!UnloadedEntityIds.Any())
			{
				return;
			}

			var dbEntityReader = new DbEntityReader<TEntity>(database);
			var entities = dbEntityReader.AsQueryable().WhereIdMatches(UnloadedEntityIds);

			foreach (var entity in entities)
			{
				Update(entity, DbEntityEntryState.NoChanges);
			}

			UnloadedEntityIds = Enumerable.Empty<object>();
		}

		public void WriteChanges(IMongoDatabase database)
		{
			DetectChanges();

			var dbEntityWriter = new DbEntityWriter<TEntity>(database);
			dbEntityWriter.Write(this);

			CommitChanges();
		}
	}
}
