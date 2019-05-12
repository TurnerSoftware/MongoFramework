using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Mapping;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace MongoFramework.Tests
{
	[TestClass]
	public abstract class TestBase
	{
		protected static void ResetMongoDb()
		{
			ResetMongoDbDriver();
			EntityMapping.RemoveAllDefinitions();
		}

		private static void ResetMongoDbDriver()
		{
			//Primarily introduced to better test TypeDiscoverySerializer, this is designed to reset the MongoDB driver
			//as if the assembly just loaded. It is likely incomplete and would be easily subject to breaking in future 
			//driver updates. If someone knows a better way to reset the MongoDB driver, please open a pull request!

			var classMapField = typeof(BsonClassMap).GetField("__classMaps", BindingFlags.NonPublic | BindingFlags.Static);
			classMapField.SetValue(null, new Dictionary<Type, BsonClassMap>());

			var knownTypesField = typeof(BsonSerializer).GetField("__typesWithRegisteredKnownTypes", BindingFlags.NonPublic | BindingFlags.Static);
			knownTypesField.SetValue(null, new HashSet<Type>());

			var discriminatorTypesField = typeof(BsonSerializer).GetField("__discriminatedTypes", BindingFlags.NonPublic | BindingFlags.Static);
			discriminatorTypesField.SetValue(null, new HashSet<Type>());

			var discriminatorsField = typeof(BsonSerializer).GetField("__discriminators", BindingFlags.NonPublic | BindingFlags.Static);
			discriminatorsField.SetValue(null, new Dictionary<BsonValue, HashSet<Type>>());

			var serializerRegistryField = typeof(BsonSerializer).GetField("__serializerRegistry", BindingFlags.NonPublic | BindingFlags.Static);
			if (serializerRegistryField.GetValue(null) is BsonSerializerRegistry registry)
			{
				var cacheField = typeof(BsonSerializerRegistry).GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance);
				var registryCache = cacheField.GetValue(registry) as ConcurrentDictionary<Type, IBsonSerializer>;
				registryCache.Clear();
			}
		}

		protected static void ClearDatabase()
		{
			//Removing the database created for the tests
			var client = new MongoClient(TestConfiguration.ConnectionString);
			client.DropDatabase(TestConfiguration.GetDatabaseName());
		}

		[TestInitialize]
		public void Initialise()
		{
			ResetMongoDb();
			ClearDatabase();
		}

		[AssemblyCleanup]
		public static void AssemblyCleanup()
		{
			ClearDatabase();
		}
	}
}