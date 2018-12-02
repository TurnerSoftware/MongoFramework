using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoFramework.Infrastructure
{
	public class EntityBucketCollection<TGroup, TSubEntity> where TGroup : class
	{
		private Dictionary<TGroup, List<TSubEntity>> SubEntityStaging { get; }
		private IEntityReader<EntityBucket<TGroup, TSubEntity>> EntityReader { get; }

		public int BucketSize { get; }

		public EntityBucketCollection(IEntityReader<EntityBucket<TGroup, TSubEntity>> entityReader, int bucketSize)
		{
			SubEntityStaging = new Dictionary<TGroup, List<TSubEntity>>(new ShallowPropertyEqualityComparer<TGroup>());
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

		public IEntityCollection<EntityBucket<TGroup, TSubEntity>> AsEntityCollection()
		{
			var entityCollection = new EntityCollection<EntityBucket<TGroup, TSubEntity>>();

			foreach (var grouping in SubEntityStaging)
			{
				var entityList = grouping.Value;
				var sliceAt = 0;
				var currentBucketIndex = 1;

				var remainingEntitiesCount = entityList.Count;

				//Identify last bucket of the group for the last index and to potentially backfill into it (if there is space)
				var bucket = EntityReader.AsQueryable().Where(e => e.Group == grouping.Key).OrderByDescending(e => e.Index).FirstOrDefault();
				if (bucket != null)
				{
					//Check if there is room to backfill into the existing bucket
					if (bucket.ItemCount < bucket.BucketSize)
					{
						var sliceSize = Math.Min(bucket.BucketSize - bucket.ItemCount, remainingEntitiesCount);
						var sliceEntities = entityList.Take(sliceSize).ToArray();

						for (int i = 0, l = sliceEntities.Length; i < l; i++)
						{
							bucket.Items[bucket.ItemCount + i] = sliceEntities[i];
						}

						bucket.ItemCount += sliceSize;

						entityCollection.Update(bucket, EntityEntryState.Updated);

						sliceAt += sliceSize;
						remainingEntitiesCount -= sliceSize;
					}

					currentBucketIndex = bucket.Index + 1;
				}

				while (remainingEntitiesCount > 0)
				{
					var sliceSize = Math.Min(BucketSize, remainingEntitiesCount);
					var sliceEntities = entityList.Skip(sliceAt).Take(sliceSize).ToArray();

					var remainingSpace = BucketSize - sliceSize;
					if (remainingSpace > 0)
					{
						Array.Resize(ref sliceEntities, BucketSize);
						for (int i = sliceSize, l = BucketSize; i < l; i++)
						{
							sliceEntities[i] = Activator.CreateInstance<TSubEntity>();
						}
					}

					entityCollection.Add(new EntityBucket<TGroup, TSubEntity>
					{
						Group = grouping.Key,
						Index = currentBucketIndex,
						ItemCount = sliceSize,
						BucketSize = BucketSize,
						Items = sliceEntities
					});

					currentBucketIndex++;

					sliceAt += sliceSize;
					remainingEntitiesCount -= sliceSize;
				}
			}

			return entityCollection;
		}

		public void Clear()
		{
			SubEntityStaging.Clear();
		}
	}
}
