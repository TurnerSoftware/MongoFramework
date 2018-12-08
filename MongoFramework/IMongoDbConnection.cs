using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Indexing;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework
{
	public interface IMongoDbConnection : IEntityMapperFactory, IEntityIndexMapperFactory, IDisposable
	{
		IMongoClient Client { get; }
		IMongoDatabase GetDatabase();
	}
}
