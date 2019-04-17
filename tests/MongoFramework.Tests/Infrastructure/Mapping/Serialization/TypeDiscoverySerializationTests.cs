using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoFramework.Attributes;
using MongoFramework.Infrastructure.Mapping.Serialization;
using System.Collections.Generic;

namespace MongoFramework.Tests.Infrastructure.Mapping.Serialization
{
	[TestClass]
	public class TypeDiscoverySerializationTests : TestBase
	{
		public class CollectionBaseModel
		{
			public IList<KnownBaseModel> KnownList { get; set; }
		}

		[RuntimeTypeDiscovery]
		public class KnownBaseModel
		{
		}

		public class UnknownChildModel : KnownBaseModel
		{
		}

		public class UnknownGrandChildModel : UnknownChildModel
		{
		}

		[TestMethod]
		public void NullDeserialization()
		{
			var connection = TestConfiguration.GetConnection();
			var entityMapper = connection.GetEntityMapper(typeof(KnownBaseModel));
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
			var connection = TestConfiguration.GetConnection();
			var entityMapper = connection.GetEntityMapper(typeof(KnownBaseModel));

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
			var connection = TestConfiguration.GetConnection();
			var entityMapper = connection.GetEntityMapper(typeof(KnownBaseModel));

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
			var connection = TestConfiguration.GetConnection();
			var entityMapper = connection.GetEntityMapper(typeof(KnownBaseModel));

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
			var connection = TestConfiguration.GetConnection();
			var entityMapper = connection.GetEntityMapper(typeof(CollectionBaseModel));

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
			var connection = TestConfiguration.GetConnection();
			var entityMapper = connection.GetEntityMapper(typeof(CollectionBaseModel));

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
