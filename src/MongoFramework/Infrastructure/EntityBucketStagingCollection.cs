using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MongoFramework.Infrastructure.Linq;
using MongoFramework.Infrastructure.Linq.Processors;

namespace MongoFramework.Infrastructure
{
	public class EntityBucketStagingCollection<TGroup, TSubEntity> : IEntityCollection<EntityBucket<TGroup, TSubEntity>> where TGroup : class
	{
		private Dictionary<TGroup, List<TSubEntity>> SubEntityStaging { get; }
		private IEntityCollection<EntityBucket<TGroup, TSubEntity>> ChangeTracker { get; }
		private IEntityReader<EntityBucket<TGroup, TSubEntity>> EntityReader { get; }

		public int BucketSize { get; }

		public int Count => throw new NotImplementedException();

		public bool IsReadOnly => false;

		public EntityBucketStagingCollection(IEntityReader<EntityBucket<TGroup, TSubEntity>> entityReader, int bucketSize)
		{
			SubEntityStaging = new Dictionary<TGroup, List<TSubEntity>>(new ShallowPropertyEqualityComparer<TGroup>());
			ChangeTracker = new EntityCollection<EntityBucket<TGroup, TSubEntity>>();
			EntityReader = entityReader;
			BucketSize = bucketSize;
		}

		public void AddEntity(TGroup group, TSubEntity entity)
		{
			if (!SubEntityStaging.ContainsKey(group))
			{
				SubEntityStaging.Add(group, new List<TSubEntity> { entity });
			}
			else
			{
				SubEntityStaging[group].Add(entity);
			}
		}

		public void Clear()
		{
			ChangeTracker.Clear();
			SubEntityStaging.Clear();
		}

		private IQueryable<EntityBucket<TGroup, TSubEntity>> QueryDatabase()
		{
			var queryable = EntityReader.AsQueryable() as IMongoFrameworkQueryable<EntityBucket<TGroup, TSubEntity>, EntityBucket<TGroup, TSubEntity>>;
			queryable.EntityProcessors.Add(new EntityTrackingProcessor<EntityBucket<TGroup, TSubEntity>>(ChangeTracker));
			return queryable;
		}

		public IEnumerable<EntityEntry<EntityBucket<TGroup, TSubEntity>>> GetEntries()
		{
			foreach (var grouping in SubEntityStaging)
			{
				var entityList = grouping.Value;
				var sliceAt = 0;
				var currentBucketIndex = 1;

				var remainingEntitiesCount = entityList.Count;

				//Identify last bucket of the group for the last index and to potentially backfill into it (if there is space)
				var bucket = QueryDatabase().Where(e => e.Group == grouping.Key).OrderByDescending(e => e.Index).FirstOrDefault();
				if (bucket != null)
				{
					//Check if there is room to backfill into the existing bucket
					if (bucket.ItemCount < bucket.BucketSize)
					{
						var sliceSize = Math.Min(bucket.BucketSize - bucket.ItemCount, remainingEntitiesCount);
						var sliceEntities = entityList.Take(sliceSize).ToArray();

						bucket.Items.AddRange(sliceEntities);
						bucket.ItemCount += sliceSize;

						yield return new EntityEntry<EntityBucket<TGroup, TSubEntity>>(bucket, EntityEntryState.Updated);

						sliceAt += sliceSize;
						remainingEntitiesCount -= sliceSize;
					}

					currentBucketIndex = bucket.Index + 1;
				}

				while (remainingEntitiesCount > 0)
				{
					var sliceSize = Math.Min(BucketSize, remainingEntitiesCount);
					var sliceEntities = entityList.Skip(sliceAt).Take(sliceSize).ToList();

					yield return new EntityEntry<EntityBucket<TGroup, TSubEntity>>(new EntityBucket<TGroup, TSubEntity>
					{
						Group = grouping.Key,
						Index = currentBucketIndex,
						ItemCount = sliceSize,
						BucketSize = BucketSize,
						Items = sliceEntities
					}, EntityEntryState.Added);

					currentBucketIndex++;

					sliceAt += sliceSize;
					remainingEntitiesCount -= sliceSize;
				}
			}
		}

		public EntityEntry<EntityBucket<TGroup, TSubEntity>> GetEntry(EntityBucket<TGroup, TSubEntity> entity)
		{
			throw new NotImplementedException();
		}

		public void Update(EntityBucket<TGroup, TSubEntity> entity, EntityEntryState state)
		{
			throw new NotImplementedException();
		}

		public void Add(EntityBucket<TGroup, TSubEntity> item)
		{
			throw new NotImplementedException();
		}

		public bool Contains(EntityBucket<TGroup, TSubEntity> item)
		{
			throw new NotImplementedException();
		}

		public void CopyTo(EntityBucket<TGroup, TSubEntity>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public bool Remove(EntityBucket<TGroup, TSubEntity> item)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<EntityBucket<TGroup, TSubEntity>> GetEnumerator()
		{
			var result = GetEntries().Select(e => e.Entity);
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
