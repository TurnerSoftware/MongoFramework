using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure.EntityRelationships;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MongoFramework.Tests.EntityRelationships
{
	[TestClass]
	public class EntityNavigationCollectionSerializerTests
	{
		[TestMethod]
		public void DeserializingNullReturnsCollection()
		{
			var serializer = new EntityNavigationCollectionSerializer<StringIdModel>("Id");

			var document = new BsonDocument(new Dictionary<string, object>
			{
				{ "Items", null }
			});

			using (var reader = new BsonDocumentReader(document))
			{
				reader.ReadBsonType();
				reader.ReadStartDocument();
				reader.ReadBsonType();
				reader.SkipName();

				var context = BsonDeserializationContext.CreateRoot(reader);
				var result = serializer.Deserialize(context);

				Assert.IsNotNull(result);
				Assert.IsInstanceOfType(result, typeof(EntityNavigationCollection<StringIdModel>));
			}
		}

		[TestMethod]
		[ExpectedException(typeof(NotSupportedException))]
		public void DeserializingInvalidTypeThrowsException()
		{
			var serializer = new EntityNavigationCollectionSerializer<StringIdModel>("Id");

			var document = new BsonDocument(new Dictionary<string, object>
			{
				{ "Items", false }
			});

			using (var reader = new BsonDocumentReader(document))
			{
				reader.ReadBsonType();
				reader.ReadStartDocument();
				reader.ReadBsonType();
				reader.SkipName();

				var context = BsonDeserializationContext.CreateRoot(reader);
				serializer.Deserialize(context);
			}
		}

		[TestMethod]
		public void ReserializingStringIdEntityMaintainsStateExceptNulls()
		{
			var serializer = new EntityNavigationCollectionSerializer<StringIdModel>("Id");

			var initialCollection = new EntityNavigationCollection<StringIdModel>("Id")
			{
				new StringIdModel
				{
					Description = "1"
				},
				new StringIdModel
				{
					Id = "5ac383379a5f1303784400f8",
					Description = "2"
				}
			};
			EntityNavigationCollection<StringIdModel> deserializedCollection = null;

			initialCollection.AddForeignId("5ac383379a5f1303784400f9");

			var document = new BsonDocument();

			using (var writer = new BsonDocumentWriter(document))
			{
				writer.WriteStartDocument();
				writer.WriteName("Items");

				var context = BsonSerializationContext.CreateRoot(writer);
				serializer.Serialize(context, initialCollection);

				writer.WriteEndDocument();
			}

			using (var reader = new BsonDocumentReader(document))
			{
				reader.ReadBsonType();
				reader.ReadStartDocument();
				reader.ReadBsonType();
				reader.SkipName();

				var context = BsonDeserializationContext.CreateRoot(reader);
				deserializedCollection = serializer.Deserialize(context) as EntityNavigationCollection<StringIdModel>;
			}

			Assert.AreEqual(3, initialCollection.GetForeignIds().Count());
			Assert.AreEqual(2, deserializedCollection.GetForeignIds().Count());
			Assert.IsTrue(deserializedCollection.GetForeignIds().All(id => initialCollection.GetForeignIds().Contains(id)));
		}

		[TestMethod]
		public void ReserializingObjectIdIdEntityMaintainsState()
		{
			var serializer = new EntityNavigationCollectionSerializer<ObjectIdIdModel>("Id");

			var initialCollection = new EntityNavigationCollection<ObjectIdIdModel>("Id")
			{
				new ObjectIdIdModel
				{
					Id = ObjectId.GenerateNewId(),
					Description = "1"
				}
			};
			EntityNavigationCollection<ObjectIdIdModel> deserializedCollection = null;

			initialCollection.AddForeignId(ObjectId.GenerateNewId());

			var document = new BsonDocument();

			using (var writer = new BsonDocumentWriter(document))
			{
				writer.WriteStartDocument();
				writer.WriteName("Items");

				var context = BsonSerializationContext.CreateRoot(writer);
				serializer.Serialize(context, initialCollection);

				writer.WriteEndDocument();
			}

			using (var reader = new BsonDocumentReader(document))
			{
				reader.ReadBsonType();
				reader.ReadStartDocument();
				reader.ReadBsonType();
				reader.SkipName();

				var context = BsonDeserializationContext.CreateRoot(reader);
				deserializedCollection = serializer.Deserialize(context) as EntityNavigationCollection<ObjectIdIdModel>;
			}

			Assert.AreEqual(2, initialCollection.GetForeignIds().Count());
			Assert.AreEqual(2, deserializedCollection.GetForeignIds().Count());
			Assert.IsTrue(initialCollection.GetForeignIds().All(id => deserializedCollection.GetForeignIds().Contains(id)));
		}

		[TestMethod]
		public void SerializeICollectionCompatibleButIsntEntityNavigationCollection()
		{
			var serializer = new EntityNavigationCollectionSerializer<ObjectIdIdModel>("Id");

			var initialCollection = new List<ObjectIdIdModel>
			{
				new ObjectIdIdModel
				{
					Id = ObjectId.GenerateNewId(),
					Description = "1"
				}
			};
			EntityNavigationCollection<ObjectIdIdModel> deserializedCollection = null;

			var document = new BsonDocument();

			using (var writer = new BsonDocumentWriter(document))
			{
				writer.WriteStartDocument();
				writer.WriteName("Items");

				var context = BsonSerializationContext.CreateRoot(writer);
				serializer.Serialize(context, initialCollection);

				writer.WriteEndDocument();
			}

			using (var reader = new BsonDocumentReader(document))
			{
				reader.ReadBsonType();
				reader.ReadStartDocument();
				reader.ReadBsonType();
				reader.SkipName();

				var context = BsonDeserializationContext.CreateRoot(reader);
				deserializedCollection = serializer.Deserialize(context) as EntityNavigationCollection<ObjectIdIdModel>;
			}

			Assert.AreEqual(1, initialCollection.Count());
			Assert.AreEqual(1, deserializedCollection.GetForeignIds().Count());
			Assert.IsTrue(initialCollection.Select(e => e.Id).All(id => deserializedCollection.GetForeignIds().Contains(id)));
		}

		[TestMethod, ExpectedException(typeof(NotSupportedException))]
		public void SerializeUnsupportedType()
		{
			var serializer = new EntityNavigationCollectionSerializer<ObjectIdIdModel>("Id");

			var initialCollection = new List<DateTime>
			{
				DateTime.Now
			};

			var document = new BsonDocument();

			using (var writer = new BsonDocumentWriter(document))
			{
				writer.WriteStartDocument();
				writer.WriteName("Items");

				var context = BsonSerializationContext.CreateRoot(writer);
				serializer.Serialize(context, initialCollection);

				writer.WriteEndDocument();
			}
		}
	}
}
