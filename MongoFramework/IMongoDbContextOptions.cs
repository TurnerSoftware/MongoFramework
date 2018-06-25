using System;
using System.Collections.Generic;
using System.Text;

namespace MongoFramework
{
	public interface IMongoDbContextOptions
	{
		string ConnectionString { get; }
		string Database { get; }
	}
}
