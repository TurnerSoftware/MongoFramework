using System;
using MongoDB.Driver;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Diagnostics;

namespace MongoFramework
{
	public interface IMongoDbConnection : IDisposable
	{
		IMongoClient Client { get; }
		IMongoDatabase GetDatabase();
		IDiagnosticListener DiagnosticListener { set; get; }
	}
}
