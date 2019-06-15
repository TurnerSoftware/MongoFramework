using System;
using MongoDB.Driver;

namespace MongoFramework.Tests
{
	static class TestConfiguration
	{
		public static string ConnectionString => Environment.GetEnvironmentVariable("MONGODB_URI") ?? "mongodb://localhost";

		public static string GetDatabaseName()
		{
			return "MongoFrameworkTests";
		}

		public static IMongoDbConnection GetConnection()
		{
			var urlBuilder = new MongoUrlBuilder(ConnectionString)
			{
				DatabaseName = GetDatabaseName()
			};
			return MongoDbConnection.FromUrl(urlBuilder.ToMongoUrl());
		}
	}
}