using System;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoFramework.Infrastructure
{
	public static class MongoDbUtility
	{
		public static MongoUrl GetMongoUrlFromConfig(string connectionName)
		{
			var connectionStringConfig = System.Configuration.ConfigurationManager.ConnectionStrings[connectionName];

			if (connectionStringConfig != null)
			{
				return MongoUrl.Create(connectionStringConfig.ConnectionString);
			}

			return null;
		}

		public static IMongoDatabase GetDatabase(MongoUrl mongoUrl)
		{
			if (mongoUrl == null)
			{
				throw new ArgumentNullException(nameof(mongoUrl));
			}

			return GetDatabase(mongoUrl.Url, mongoUrl.DatabaseName);
		}

		public static IMongoDatabase GetDatabase(string connectionString, string databaseName)
		{
			var client = new MongoClient(connectionString);
			var database = client.GetDatabase(databaseName);
			return database;
		}

		/// <summary>
		/// Checks whether the provided string matches the 24-character hexadecimal format of an ObjectId
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static bool IsValidObjectId(string id)
		{
			return ObjectId.TryParse(id, out ObjectId result);
		}
	}
}
