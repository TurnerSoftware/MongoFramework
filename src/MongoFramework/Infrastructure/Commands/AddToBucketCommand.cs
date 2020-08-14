using System;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Infrastructure.Commands
{
	public class AddToBucketCommand<TGroup, TSubEntity> : IWriteCommand<EntityBucket<TGroup, TSubEntity>> 
		where TGroup : class
		where TSubEntity : class
	{
		private TGroup Group { get; }
		private TSubEntity SubEntity { get; }
		private IEntityProperty EntityTimeProperty { get; }
		private int BucketSize { get; }

		public AddToBucketCommand(TGroup group, TSubEntity subEntity, IEntityProperty entityTimeProperty, int bucketSize)
		{
			Group = group;
			SubEntity = subEntity;
			BucketSize = bucketSize;
			EntityTimeProperty = entityTimeProperty;
		}

		public IEnumerable<WriteModel<EntityBucket<TGroup, TSubEntity>>> GetModel()
		{
			var filterBuilder = Builders<EntityBucket<TGroup, TSubEntity>>.Filter;
			var filter = filterBuilder.And(
				filterBuilder.Eq(b => b.Group, Group),
				filterBuilder.Where(b => b.ItemCount < BucketSize)
			);

			var entityDefinition = EntityMapping.GetOrCreateDefinition(typeof(EntityBucket<TGroup, TSubEntity>));

			var itemTimeValue = (DateTime)EntityTimeProperty.GetValue(SubEntity);

			var updateDefinition = Builders<EntityBucket<TGroup, TSubEntity>>.Update
				.Inc(b => b.ItemCount, 1)
				.Push(b => b.Items, SubEntity)
				.Min(b => b.Min, itemTimeValue)
				.Max(b => b.Max, itemTimeValue)
				.SetOnInsert(b => b.BucketSize, BucketSize)
				.SetOnInsert(b => b.Id, entityDefinition.KeyGenerator.Generate());

			yield return new UpdateOneModel<EntityBucket<TGroup, TSubEntity>>(filter, updateDefinition)
			{
				IsUpsert = true
			};
		}
	}
}
