using System;
using System.Collections.Generic;
using System.Text;
using MongoFramework.Infrastructure;

namespace MongoFramework.Attributes
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
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
