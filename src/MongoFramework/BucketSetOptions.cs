using System;
using System.Collections.Generic;
using System.Text;

namespace MongoFramework
{
	public class BucketSetOptions : IDbSetOptions
	{
		public int BucketSize { get; set; }
	}
}
