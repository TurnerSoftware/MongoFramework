using System;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Diagnostics;

namespace MongoFramework
{
	public interface IMongoDbConnection : IDisposable
	{
		IMongoClient Client { get; }
		IMongoDatabase GetDatabase();
		IDiagnosticListener DiagnosticListener { get; set; }
	}
}
