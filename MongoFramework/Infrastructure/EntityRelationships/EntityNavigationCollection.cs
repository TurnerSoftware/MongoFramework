using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Linq;

namespace MongoFramework.Infrastructure.EntityRelationships
{
	public class EntityNavigationCollection<TEntity> : DbEntityChangeTracker<TEntity>, IEntityNavigationCollection<TEntity>
	{
		private IEnumerable<string> ImportEntityIds { get; set; }

		public void BeginImport(IEnumerable<string> entityIds)
		{
			ImportEntityIds = entityIds;
		}

		public void FinaliseImport(IMongoDatabase database)
		{
			if (!ImportEntityIds.Any())
			{
				return;
			}

			var dbEntityReader = new DbEntityReader<TEntity>(database);
			var entities = dbEntityReader.AsQueryable().WhereIdMatches(ImportEntityIds);

			foreach (var entity in entities)
			{
				Update(entity, DbEntityEntryState.NoChanges);
			}

			ImportEntityIds = Enumerable.Empty<string>();
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
