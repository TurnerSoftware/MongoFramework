using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoFramework.Infrastructure;

namespace MongoFramework
{
	public interface IMongoDbBucketSet<TGroup, TSubEntity> : IMongoDbSet, IQueryable<EntityBucket<TGroup, TSubEntity>> where TGroup : class
	{
		void Add(TGroup group, TSubEntity entity);
		void AddRange(TGroup group, IEnumerable<TSubEntity> entities);
		IQueryable<TSubEntity> WithGroup(TGroup group);
		IQueryable<TGroup> Groups();
	}
}
