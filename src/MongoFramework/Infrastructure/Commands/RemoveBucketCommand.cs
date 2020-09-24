using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;

namespace MongoFramework.Infrastructure.Commands
{
	public class RemoveBucketCommand<TGroup, TSubEntity> : IWriteCommand<EntityBucket<TGroup, TSubEntity>>
		where TGroup : class
		where TSubEntity : class
	{
		private TGroup Group { get; }

		public Type EntityType => typeof(EntityBucket<TGroup, TSubEntity>);

		public RemoveBucketCommand(TGroup group)
		{
			Group = group;
		}

		public IEnumerable<WriteModel<EntityBucket<TGroup, TSubEntity>>> GetModel(WriteModelOptions options)
		{
			var filterBuilder = Builders<EntityBucket<TGroup, TSubEntity>>.Filter;
			var filter = filterBuilder.Eq(b => b.Group, Group);
			yield return new DeleteManyModel<EntityBucket<TGroup, TSubEntity>>(filter);
		}
	}
}
