using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace MongoFramework.Benchmarks
{
	public static class MongoDbDriverHelper
	{
		public static void ResetDriver()
		{
			//Primarily introduced to better test TypeDiscoverySerializer, this is designed to reset the MongoDB driver
			//as if the assembly just loaded. It is likely incomplete and would be easily subject to breaking in future
			//driver updates. If someone knows a better way to reset the MongoDB driver, please open a pull request!

			var classMapField = typeof(BsonClassMap).GetField("__classMaps", BindingFlags.NonPublic | BindingFlags.Static);
			if (classMapField.GetValue(null) is Dictionary<Type, BsonClassMap> classMaps)
			{
				classMaps.Clear();
			}

			var knownTypesField = typeof(BsonSerializer).GetField("__typesWithRegisteredKnownTypes", BindingFlags.NonPublic | BindingFlags.Static);
			if (knownTypesField.GetValue(null) is HashSet<Type> knownTypes)
			{
				knownTypes.Clear();
			}

			var discriminatorTypesField = typeof(BsonSerializer).GetField("__discriminatedTypes", BindingFlags.NonPublic | BindingFlags.Static);
			if (discriminatorTypesField.GetValue(null) is HashSet<Type> discriminatorTypes)
			{
				discriminatorTypes.Clear();
			}

			var discriminatorsField = typeof(BsonSerializer).GetField("__discriminators", BindingFlags.NonPublic | BindingFlags.Static);
			if (discriminatorsField.GetValue(null) is Dictionary<BsonValue, HashSet<Type>> discriminators)
			{
				discriminators.Clear();
			}

			var serializerRegistryField = typeof(BsonSerializer).GetField("__serializerRegistry", BindingFlags.NonPublic | BindingFlags.Static);
			if (serializerRegistryField.GetValue(null) is BsonSerializerRegistry registry)
			{
				var cacheField = typeof(BsonSerializerRegistry).GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance);
				var registryCache = cacheField.GetValue(registry) as ConcurrentDictionary<Type, IBsonSerializer>;
				registryCache.Clear();
			}
		}
	}
}
