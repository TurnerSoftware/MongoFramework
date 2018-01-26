using System;
using MongoDB.Driver;

namespace MongoFramework.Tests
{
	internal static class TestConfiguration
	{
		public static string ConnectionString => Environment.GetEnvironmentVariable("MONGODB_URI") ?? "mongodb://localhost";

		private static string DatabaseName { get; set; }

		public static string GetDatabaseName()
		{
			return DatabaseName ?? (DatabaseName = $"MongoFrameworkTests-{DateTime.Now:ddMMHHmm}");
		}

		public static IMongoDatabase GetDatabase()
		{
			var client = new MongoClient(ConnectionString);
			var database = client.GetDatabase(GetDatabaseName());
			return database;
		}
	}
}