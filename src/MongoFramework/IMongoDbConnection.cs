using System;
using MongoDB.Driver;
using MongoFramework.Infrastructure;

namespace MongoFramework
{
	public interface IMongoDbConnection : IDisposable
	{
		IMongoClient Client { get; }
		IMongoDatabase GetDatabase();
		IDiagnosticListener DiagnosticListener { set; get; }
	}
}
