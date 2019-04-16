using System;
using MongoDB.Driver;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Indexing;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework
{
	public interface IMongoDbConnection : IEntityMapperFactory, IEntityIndexMapperFactory, IDisposable
	{
		IMongoClient Client { get; }
		IMongoDatabase GetDatabase();
		IDiagnosticListener DiagnosticListener { get; }
	}
}
