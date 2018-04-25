using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mapping.Serialization;

namespace MongoFramework.Tests.Mapping.Serialization.TypeDiscovery
{
	[TestClass]
	public class TypeDiscoverySerializationTests
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
		public void NullDeserialization()
		{
			var entityMapper = new EntityMapper<KnownBaseModel>();
			var serializer = TypeDiscoverySerializationProvider.Instance.GetSerializer(typeof(KnownBaseModel));

			var document = new BsonDocument
			{
				{ "Item", BsonNull.Value }
			};

			KnownBaseModel deserializedResult;

			using (var reader = new BsonDocumentReader(document))
			{
				reader.ReadBsonType();
				reader.ReadStartDocument();
				reader.ReadBsonType();
				reader.SkipName();

				var context = BsonDeserializationContext.CreateRoot(reader);
				deserializedResult = (KnownBaseModel)serializer.Deserialize(context);
			}

			Assert.AreEqual(null, deserializedResult);
		}

		[TestMethod]
		public void DeserializeChildTypeDiscoveryForRootEntity()
		{
			var entityMapper = new EntityMapper<KnownBaseModel>();

			var document = new BsonDocument
			{
				{ "_t", "UnknownChildModel" }
			};

			var deserializedResult = BsonSerializer.Deserialize<KnownBaseModel>(document);
			Assert.AreEqual(typeof(UnknownChildModel), deserializedResult.GetType());
		}

		[TestMethod]
		public void DeserializeGrandChildTypeDiscoveryForRootEntity()
		{
			var entityMapper = new EntityMapper<KnownBaseModel>();

			var document = new BsonDocument
			{
				{ "_t", new BsonArray(new [] { "KnownBaseModel", "UnknownChildModel", "UnknownGrandChildModel" }) }
			};

			var deserializedResult = BsonSerializer.Deserialize<KnownBaseModel>(document);
			Assert.AreEqual(typeof(UnknownGrandChildModel), deserializedResult.GetType());
		}

		[TestMethod]
		public void DeserializeWithoutTypeDiscovery()
		{
			var entityMapper = new EntityMapper<KnownBaseModel>();

			TypeDiscoverySerializationProvider.Instance.Enabled = false;

			var document = new BsonDocument
			{
				{ "_t", "UnknownChildModel" }
			};

			var deserializedResult = BsonSerializer.Deserialize<KnownBaseModel>(document);
			Assert.AreNotEqual(typeof(UnknownChildModel), deserializedResult.GetType());

			TypeDiscoverySerializationProvider.Instance.Enabled = true;
		}

		[TestMethod]
		public void DeserializeCollection()
		{
			var entityMapper = new EntityMapper<CollectionBaseModel>();

			var document = new BsonDocument
			{
				{ "_t", "CollectionBaseModel" },
				{
					"KnownList",
					new BsonArray
					{
						new BsonDocument
						{
							{ "_t", "KnownBaseModel" }
						},
						new BsonDocument
						{
							{ "_t", "UnknownChildModel" }
						},
						new BsonDocument
						{
							{ "_t", new BsonArray(new [] { "KnownBaseModel", "UnknownChildModel", "UnknownGrandChildModel" }) }
						}
					}
				}
			};
			
			var deserializedResult = BsonSerializer.Deserialize<CollectionBaseModel>(document);
			Assert.AreEqual(typeof(KnownBaseModel), deserializedResult.KnownList[0].GetType());
			Assert.AreEqual(typeof(UnknownChildModel), deserializedResult.KnownList[1].GetType());
			Assert.AreEqual(typeof(UnknownGrandChildModel), deserializedResult.KnownList[2].GetType());
		}

		[TestMethod]
		public void ReserializationWithoutDataLoss()
		{
			var entityMapper = new EntityMapper<CollectionBaseModel>();

			var initialEntity = new CollectionBaseModel
			{
				KnownList = new List<KnownBaseModel>
				{
					new KnownBaseModel(),
					new UnknownChildModel(),
					new UnknownGrandChildModel()
				}
			};

			var document = new BsonDocument();

			using (var writer = new BsonDocumentWriter(document))
			{
				BsonSerializer.Serialize(writer, initialEntity);
			}

			var deserializedResult = BsonSerializer.Deserialize<CollectionBaseModel>(document);

			Assert.AreEqual(3, deserializedResult.KnownList.Count);
			Assert.AreEqual(typeof(KnownBaseModel), deserializedResult.KnownList[0].GetType());
			Assert.AreEqual(typeof(UnknownChildModel), deserializedResult.KnownList[1].GetType());
			Assert.AreEqual(typeof(UnknownGrandChildModel), deserializedResult.KnownList[2].GetType());
		}
	}
}
