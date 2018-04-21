using System;
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

namespace MongoFramework.Tests.Mapping.Serializers.TypeDiscoveryArray
{
	[TestClass]
	public class TypeDiscoveryArraySerializerTests
	{
		[TestInitialize]
		public void ResetMongoDbTypeCache()
		{
			var classMapField = typeof(BsonClassMap).GetField("__classMaps", BindingFlags.NonPublic | BindingFlags.Static);
			classMapField.SetValue(null, new Dictionary<Type, BsonClassMap>());

			var knownTypesField = typeof(BsonSerializer).GetField("__typesWithRegisteredKnownTypes", BindingFlags.NonPublic | BindingFlags.Static);
			knownTypesField.SetValue(null, new HashSet<Type>());

			var discriminatorTypesField = typeof(BsonSerializer).GetField("__discriminatedTypes", BindingFlags.NonPublic | BindingFlags.Static);
			discriminatorTypesField.SetValue(null, new HashSet<Type>());

			var discriminatorsField = typeof(BsonSerializer).GetField("__discriminators", BindingFlags.NonPublic | BindingFlags.Static);
			discriminatorsField.SetValue(null, new Dictionary<BsonValue, HashSet<Type>>());
		}

		[TestMethod, ExpectedException(typeof(FormatException))]
		public void DeserializationFailsWithoutTypeDiscoveryWhenTypesHaventBeenSeenBefore()
		{
			//As a raw BsonDocument as serializing the normal entity allows the MongoDB driver to become aware of the type
			var document = new BsonDocument
			{
				{ "_t", "TypeDiscoveryArraySerializerModel" },
				{ "_id", BsonNull.Value },
				{
					"KnownBaseCollection", new BsonArray(new []
					{
						new BsonDocument
						{
							{ "_id", BsonNull.Value },
							{ "Description", "KnownBaseModel" }
						},
						new BsonDocument
						{
							{ "_t", "ExtendsKnownBaseModel" },
							{ "_id", BsonNull.Value },
							{ "Description", BsonNull.Value },
							{ "ExtendsKnownBaseDescription", "ExtendsKnownBaseModel" }
						},
						new BsonDocument
						{
							{ "_t", new BsonArray(new [] { "KnownBaseModel", "ExtendsKnownBaseModel", "ExtendsExtendsKnownBaseModel" })},
							{ "_id", BsonNull.Value },
							{ "Description", BsonNull.Value },
							{ "ExtendsKnownBaseDescription", BsonNull.Value },
							{ "ExtendsExtendsKnownBaseDescription", "ExtendsExtendsKnownBaseDescription" }
						}
					})
				},
				{ "KnownInterfaceBaseList", BsonNull.Value }
			};

			BsonSerializer.Deserialize<TypeDiscoveryArraySerializerModel>(document);
		}

		public void RuntimeTypeDetectionPreventsExceptions()
		{
			var entityMapper = new EntityMapper<TypeDiscoveryArraySerializerModel>();
			var entity = new TypeDiscoveryArraySerializerModel
			{
				KnownBaseCollection = new List<KnownBaseModel>
				{
					new KnownBaseModel
					{
						Description = "KnownBaseModel"
					},
					new ExtendsKnownBaseModel
					{
						ExtendsKnownBaseDescription = "ExtendsKnownBaseModel"
					},
					new ExtendsExtendsKnownBaseModel
					{
						ExtendsExtendsKnownBaseDescription = "ExtendsExtendsKnownBaseDescription"
					}
				}
			};

			var document = new BsonDocument();

			using (var writer = new BsonDocumentWriter(document))
			{
				BsonSerializer.Serialize(writer, entity);
			}
			
			ResetMongoDbTypeCache();

			var deserializedEntity = BsonSerializer.Deserialize<TypeDiscoveryArraySerializerModel>(document);

			Assert.AreEqual(entity, deserializedEntity);
		}
	}
}
