using System;
using System.Collections.Generic;

namespace MongoFramework
{
	public class EntityBucket<TGroup, TSubEntity> where TGroup : class
	{
		public string Id { get; set; }
		public TGroup Group { get; set; }
		public DateTime Min { get; set; }
		public DateTime Max { get; set; }
		public int ItemCount { get; set; }
		public int BucketSize { get; set; }
		public List<TSubEntity> Items { get; set; }
	}
}
