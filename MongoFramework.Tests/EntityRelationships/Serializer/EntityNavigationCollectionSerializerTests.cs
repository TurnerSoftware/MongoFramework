using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure.EntityRelationships;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MongoFramework.Tests.EntityRelationships.Serializer
{
	[TestClass]
	public class EntityNavigationCollectionSerializerTests
	{
		[TestMethod]
		public void DeserializingNullReturnsCollection()
		{
			var serializer = new EntityNavigationCollectionSerializer<StringIdModel>();

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
			var serializer = new EntityNavigationCollectionSerializer<StringIdModel>();

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
		public void ReserializingStringIdEntityMaintainsState()
		{
			var serializer = new EntityNavigationCollectionSerializer<StringIdModel>();

			var initialCollection = new EntityNavigationCollection<StringIdModel>
			{
				new StringIdModel
				{
					Id = "5ac383379a5f1303784400f8",
					Description = "1"
				}
			};
			EntityNavigationCollection<StringIdModel> deserializedCollection = null;

			initialCollection.AddEntityById("5ac383379a5f1303784400f9");

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

			Assert.AreEqual(2, initialCollection.GetEntityIds().Count());
			Assert.AreEqual(2, deserializedCollection.GetEntityIds().Count());
			Assert.IsTrue(initialCollection.GetEntityIds().All(id => deserializedCollection.GetEntityIds().Contains(id)));
		}

		[TestMethod]
		public void ReserializingObjectIdIdEntityMaintainsState()
		{
			var serializer = new EntityNavigationCollectionSerializer<ObjectIdIdModel>();

			var initialCollection = new EntityNavigationCollection<ObjectIdIdModel>
			{
				new ObjectIdIdModel
				{
					Id = ObjectId.GenerateNewId(),
					Description = "1"
				}
			};
			EntityNavigationCollection<ObjectIdIdModel> deserializedCollection = null;

			initialCollection.AddEntityById(ObjectId.GenerateNewId());

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

			Assert.AreEqual(2, initialCollection.GetEntityIds().Count());
			Assert.AreEqual(2, deserializedCollection.GetEntityIds().Count());
			Assert.IsTrue(initialCollection.GetEntityIds().All(id => deserializedCollection.GetEntityIds().Contains(id)));
		}
	}
}
