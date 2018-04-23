using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace MongoFramework.Tests.Mapping.Serialization.TypeDiscovery
{
	[TestClass]
	public class TypeDiscoveryIntegrationTests
	{
		[TestInitialize]
		public void ResetMongoDbDriver()
		{
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

		[TestMethod]
		public void ReadAndWriteRootEntity()
		{
			var database = TestConfiguration.GetDatabase();
			var dbSet = new MongoDbSet<RootKnownBaseModel>();
			dbSet.SetDatabase(database);

			var rootEntity = new RootKnownBaseModel
			{
				Description = "ReadAndWriteRootEntity-RootKnownBaseModel"
			};
			dbSet.Add(rootEntity);

			var childEntity = new UnknownChildToRootModel
			{
				Description = "ReadAndWriteRootEntity-UnknownChildToRootModel"
			};
			dbSet.Add(childEntity);

			dbSet.SaveChanges();

			ResetMongoDbDriver();
			dbSet = new MongoDbSet<RootKnownBaseModel>();
			dbSet.SetDatabase(database);

			var dbRootEntity = dbSet.Where(e => e.Id == rootEntity.Id).FirstOrDefault();
			Assert.IsNotNull(dbRootEntity);
			Assert.AreEqual(typeof(RootKnownBaseModel), dbRootEntity.GetType());

			var dbChildEntity = dbSet.Where(e => e.Id == childEntity.Id).FirstOrDefault();
			Assert.IsNotNull(dbChildEntity);
			Assert.AreEqual(typeof(UnknownChildToRootModel), dbChildEntity.GetType());
		}
	}
}
