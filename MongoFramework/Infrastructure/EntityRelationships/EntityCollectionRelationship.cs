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
	public class EntityCollectionRelationship<TEntity> : ICollection<TEntity>, IEntityCollectionRelationship
	{
		private DbEntityCollection<TEntity> Container { get; } = new DbEntityCollection<TEntity>();
		private IEnumerable<string> ImportEntityIds { get; set; }

		public int Count => Container.GetEntries().Count();

		public bool IsReadOnly => false;

		public void Insert(TEntity entity)
		{
			Container.Update(entity, DbEntityEntryState.NoChanges);
		}

		public void Add(TEntity item)
		{
			Container.Update(item, DbEntityEntryState.Added);
		}

		public void Clear()
		{
			Container.Clear();
		}

		public bool Contains(TEntity item)
		{
			return Container.GetEntry(item) != null;
		}

		public void CopyTo(TEntity[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<TEntity> GetEnumerator()
		{
			var result = Container.GetEntries().Select(e => e.Entity);
			using (var enumerator = result.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					yield return enumerator.Current;
				}
			}
		}

		public bool Remove(TEntity item)
		{
			Container.Update(item, DbEntityEntryState.Deleted);
			return true;
		}

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
				Container.Update(entity, DbEntityEntryState.NoChanges);
			}

			ImportEntityIds = Enumerable.Empty<string>();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
