using MongoDB.Bson;
using MongoDB.Driver;
using System;

namespace MongoFramework.Infrastructure
{
	public static class MongoDbUtility
	{
#if !NETCOREAPP2_0
		public static MongoUrl GetMongoUrlFromConfig(string connectionName)
		{
			var connectionStringConfig = System.Configuration.ConfigurationManager.ConnectionStrings[connectionName];

			if (connectionStringConfig != null)
			{
				return MongoUrl.Create(connectionStringConfig.ConnectionString);
			}

			return null;
		}
#endif

		public static IMongoDatabase GetDatabase(MongoUrl mongoUrl)
		{
			if (mongoUrl == null)
			{
				throw new ArgumentNullException(nameof(mongoUrl));
			}

			var client = new MongoClient(mongoUrl);
			var database = client.GetDatabase(mongoUrl.DatabaseName);
			return database;
		}

		public static IMongoDatabase GetDatabase(string connectionString, string databaseName)
		{
			var urlBuilder = new MongoUrlBuilder(connectionString)
			{
				DatabaseName = databaseName
			};
			return GetDatabase(urlBuilder.ToMongoUrl());
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
