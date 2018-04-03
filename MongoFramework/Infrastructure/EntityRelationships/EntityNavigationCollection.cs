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
		public IEnumerable<string> ImportIds { get; private set; } = Enumerable.Empty<string>();

		public void BeginImport(IEnumerable<string> importIds)
		{
			ImportIds = importIds;
		}

		public void FinaliseImport(IMongoDatabase database)
		{
			if (!ImportIds.Any())
			{
				return;
			}

			var dbEntityReader = new DbEntityReader<TEntity>(database);
			var entities = dbEntityReader.AsQueryable().WhereIdMatches(ImportIds);

			foreach (var entity in entities)
			{
				Update(entity, DbEntityEntryState.NoChanges);
			}

			ImportIds = Enumerable.Empty<string>();
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
