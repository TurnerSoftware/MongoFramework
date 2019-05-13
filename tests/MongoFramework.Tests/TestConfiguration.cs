using System;
using MongoDB.Driver;

namespace MongoFramework.Tests
{
	static class TestConfiguration
	{
		public static string ConnectionString => Environment.GetEnvironmentVariable("MONGODB_URI") ?? "mongodb://localhost";

		private static string DatabaseName { get; set; }

		public static string GetDatabaseName()
		{
			if (DatabaseName == null)
			{
				DatabaseName = "MongoFrameworkTests-" + DateTime.Now.ToString("ddMMHHmm");
			}

			return DatabaseName;
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