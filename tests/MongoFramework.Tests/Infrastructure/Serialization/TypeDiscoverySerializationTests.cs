using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoFramework.Attributes;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mapping.Processors;
using MongoFramework.Infrastructure.Serialization;

namespace MongoFramework.Tests.Infrastructure.Serialization
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

		public class UnknownPropertyTypeSerializationModel
		{
			public object UnknownPropertyType { get; set; }
		}

		public class UnknownPropertyTypeChildSerializationModel
		{
		}

		public class UnknownDictionaryValueModel
		{
			public IDictionary<string, object> Dictionary { get; set; }
		}

		public class NoTypeDiscovery_KnownBaseModel
		{
		}
		public class NoTypeDiscovery_UnknownChildModel : NoTypeDiscovery_KnownBaseModel
		{
		}

		[TestMethod]
		public void NullDeserialization()
		{
			EntityMapping.RegisterType(typeof(KnownBaseModel));

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

			Assert.IsNull(deserializedResult);
		}

		[TestMethod]
		public void DeserializeChildTypeDiscoveryForRootEntity()
		{
			EntityMapping.RegisterType(typeof(KnownBaseModel));

			var document = new BsonDocument
			{
				{ "_t", "UnknownChildModel" }
			};

			var deserializedResult = BsonSerializer.Deserialize<KnownBaseModel>(document);
			Assert.IsInstanceOfType(deserializedResult, typeof(UnknownChildModel));
		}

		[TestMethod]
		public void DeserializeGrandChildTypeDiscoveryForRootEntity()
		{
			EntityMapping.RegisterType(typeof(KnownBaseModel));

			var document = new BsonDocument
			{
				{ "_t", new BsonArray(new [] { "KnownBaseModel", "UnknownChildModel", "UnknownGrandChildModel" }) }
			};

			var deserializedResult = BsonSerializer.Deserialize<KnownBaseModel>(document);
			Assert.IsInstanceOfType(deserializedResult, typeof(UnknownGrandChildModel));
		}

		[TestMethod]
		public void DeserializeWithoutTypeDiscovery()
		{
			//This test primarily confirms the behaviour of the driver without type discovery
			EntityMapping.RegisterType(typeof(NoTypeDiscovery_KnownBaseModel));

			var document = new BsonDocument
			{
				{ "_t", "UnknownChildModel" }
			};

			var deserializedResult = BsonSerializer.Deserialize<NoTypeDiscovery_KnownBaseModel>(document);
			Assert.IsNotInstanceOfType(deserializedResult, typeof(NoTypeDiscovery_UnknownChildModel));
		}

		[TestMethod]
		public void DeserializeCollection()
		{
			EntityMapping.RegisterType(typeof(CollectionBaseModel));

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
			Assert.IsInstanceOfType(deserializedResult.KnownList[0], typeof(KnownBaseModel));
			Assert.IsInstanceOfType(deserializedResult.KnownList[1], typeof(UnknownChildModel));
			Assert.IsInstanceOfType(deserializedResult.KnownList[2], typeof(UnknownGrandChildModel));
		}

		[TestMethod]
		public void ReserializationWithoutDataLoss()
		{
			EntityMapping.RegisterType(typeof(CollectionBaseModel));

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
			Assert.IsInstanceOfType(deserializedResult.KnownList[0], typeof(KnownBaseModel));
			Assert.IsInstanceOfType(deserializedResult.KnownList[1], typeof(UnknownChildModel));
			Assert.IsInstanceOfType(deserializedResult.KnownList[2], typeof(UnknownGrandChildModel));
		}

		[TestMethod]
		public void DeserializeNullForUnknownPropertyType()
		{
			EntityMapping.RegisterType(typeof(UnknownPropertyTypeSerializationModel));

			var document = new BsonDocument
			{
				{ "_t", "UnknownPropertyTypeSerializationModel" },
				{ "UnknownPropertyType", BsonNull.Value }
			};

			var deserializedResult = BsonSerializer.Deserialize<UnknownPropertyTypeSerializationModel>(document);
			Assert.IsNull(deserializedResult.UnknownPropertyType);
		}

		[TestMethod]
		public void DeserializeDictionaryForUnknownPropertyType()
		{
			EntityMapping.RegisterType(typeof(UnknownPropertyTypeSerializationModel));

			var document = new BsonDocument
			{
				{ "_t", "UnknownPropertyTypeSerializationModel" },
				{
					"UnknownPropertyType",
					new BsonDocument
					{
						{ "ThisIsA", "Dictionary" }
					}
				}
			};

			var deserializedResult = BsonSerializer.Deserialize<UnknownPropertyTypeSerializationModel>(document);
			Assert.IsInstanceOfType(deserializedResult.UnknownPropertyType, typeof(Dictionary<string, object>));

			var dictionary = deserializedResult.UnknownPropertyType as Dictionary<string, object>;
			Assert.IsTrue(dictionary.ContainsKey("ThisIsA"));
			Assert.AreEqual("Dictionary", dictionary["ThisIsA"]);
		}

		[TestMethod]
		public void DeserializeSpecifiedForUnknownPropertyType()
		{
			EntityMapping.RegisterType(typeof(UnknownPropertyTypeSerializationModel));

			var document = new BsonDocument
			{
				{ "_t", "UnknownPropertyTypeSerializationModel" },
				{
					"UnknownPropertyType",
					new BsonDocument
					{
						{ "_t", "UnknownPropertyTypeChildSerializationModel" }
					}
				}
			};

			var deserializedResult = BsonSerializer.Deserialize<UnknownPropertyTypeSerializationModel>(document);
			Assert.IsInstanceOfType(deserializedResult.UnknownPropertyType, typeof(UnknownPropertyTypeChildSerializationModel));
		}

		[TestMethod]
		public void DeserializeStringForUnknownPropertyType()
		{
			EntityMapping.RegisterType(typeof(UnknownPropertyTypeSerializationModel));

			var document = new BsonDocument
			{
				{ "_t", "UnknownPropertyTypeSerializationModel" },
				{
					"UnknownPropertyType",
					new BsonString("SerializedString")
				}
			};

			var deserializedResult = BsonSerializer.Deserialize<UnknownPropertyTypeSerializationModel>(document);
			Assert.IsInstanceOfType(deserializedResult.UnknownPropertyType, typeof(string));
		}

		[TestMethod]
		public void DeserializeBooleanForUnknownPropertyType()
		{
			EntityMapping.RegisterType(typeof(UnknownPropertyTypeSerializationModel));

			var document = new BsonDocument
			{
				{ "_t", "UnknownPropertyTypeSerializationModel" },
				{
					"UnknownPropertyType",
					new BsonBoolean(true)
				}
			};

			var deserializedResult = BsonSerializer.Deserialize<UnknownPropertyTypeSerializationModel>(document);
			Assert.IsInstanceOfType(deserializedResult.UnknownPropertyType, typeof(bool));
		}

		[TestMethod]
		public void DeserializeUnknownTypesInDictionary()
		{
			EntityMapping.RegisterType(typeof(UnknownDictionaryValueModel));

			var document = new BsonDocument
			{
				{ "_t", "UnknownDictionaryValueModel" },
				{
					"Dictionary",
					new BsonDocument
					{
						{ "String", "ObjectValueAsString" },
						{ "Number", 1 },
						{ "Date", new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
						{ "Boolean", true },
						{ "Array", new BsonArray(new int[] { 10, 20, 30 }) },
						{ "ObjectId", ObjectId.Parse("507f1f77bcf86cd799439011") }
					}
				}
			};

			var result = BsonSerializer.Deserialize<UnknownDictionaryValueModel>(document);
			Assert.IsNotNull(result.Dictionary);
			Assert.AreEqual("ObjectValueAsString", result.Dictionary["String"]);
			Assert.AreEqual(1, result.Dictionary["Number"]);
			Assert.AreEqual(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc), result.Dictionary["Date"]);
			Assert.AreEqual(true, result.Dictionary["Boolean"]);
			Assert.AreEqual(20, ((object[])result.Dictionary["Array"])[1]);
			Assert.AreEqual(ObjectId.Parse("507f1f77bcf86cd799439011"), result.Dictionary["ObjectId"]);
		}
	}
}
