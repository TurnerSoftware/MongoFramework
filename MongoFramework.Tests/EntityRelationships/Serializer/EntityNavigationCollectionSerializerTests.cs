using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure.EntityRelationships;

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
		public void ReserializingMaintainsState()
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

			initialCollection.BeginImport(new[] { "5ac383379a5f1303784400f9" });

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

			Assert.AreEqual(1, initialCollection.ImportIds.Count());
			Assert.AreEqual(1, initialCollection.Count);
			Assert.IsTrue(initialCollection.All(e => deserializedCollection.ImportIds.Contains(e.Id)));
			Assert.IsTrue(initialCollection.ImportIds.All(id => deserializedCollection.ImportIds.Contains(id)));
			Assert.AreEqual(2, deserializedCollection.ImportIds.Count());
			Assert.AreEqual(0, deserializedCollection.Count);
		}
	}
}
