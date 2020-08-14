using System;

namespace MongoFramework.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class BucketSetOptionsAttribute : DbSetOptionsAttribute
	{
		public int BucketSize { get; }
		public string EntityTimeProperty { get; }

		public BucketSetOptionsAttribute(int bucketSize, string entityTimeProperty)
		{
			BucketSize = bucketSize;
			EntityTimeProperty = entityTimeProperty;
		}

		public override IDbSetOptions GetOptions()
		{
			return new BucketSetOptions
			{
				BucketSize = BucketSize,
				EntityTimeProperty = EntityTimeProperty
			};
		}
	}
}
