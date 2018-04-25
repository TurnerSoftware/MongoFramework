using System;
using System.Collections.Generic;
using System.Text;

namespace MongoFramework
{
	public class MongoDbContextOptions : IMongoDbContextOptions
	{
		public string ConnectionString { get; set; }
		public string Database { get; set; }
	}
}
