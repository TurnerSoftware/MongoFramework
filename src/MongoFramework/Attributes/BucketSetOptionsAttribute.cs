using System;

namespace MongoFramework.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class BucketSetOptionsAttribute : DbSetOptionsAttribute
	{
		public int BucketSize { get; private set; }

		public BucketSetOptionsAttribute(int bucketSize)
		{
			BucketSize = bucketSize;
		}

		public override IDbSetOptions GetOptions()
		{
			return new BucketSetOptions
			{
				BucketSize = BucketSize
			};
		}
	}
}
