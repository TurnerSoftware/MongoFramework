using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Tests
{
	static class TestConfiguration
	{
		public static string ConnectionString
		{
			get => Environment.GetEnvironmentVariable("MONGODB_URI") ?? "mongodb://localhost";
		}

		private static string DatabaseName { get; set; }
		public static string GetDatabaseName()
		{
			if (DatabaseName == null)
			{
				DatabaseName = "MongoFrameworkTests-" + DateTime.Now.ToString("ddMMHHmm");
			}

			return DatabaseName;
		}

		public static IMongoDatabase GetDatabase()
		{
			var client = new MongoClient(ConnectionString);
			var database = client.GetDatabase(GetDatabaseName());
			return database;
		}
	}
}
