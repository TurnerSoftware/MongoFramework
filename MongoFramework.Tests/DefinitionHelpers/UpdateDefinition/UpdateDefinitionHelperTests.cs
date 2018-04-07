using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoFramework.Infrastructure.DefinitionHelpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MongoFramework.Tests.DefinitionHelpers.UpdateDefinition
{
	[TestClass]
	public class UpdateDefinitionHelperTests : DbTest
	{
		private BsonDocument PerformUpdateAgainstServer(BsonDocument original, UpdateDefinition<BsonDocument> updateDefinition)
		{
			var collection = TestConfiguration.GetDatabase().GetCollection<BsonDocument>("UpdateDefinitionHelperTests");
			collection.InsertOne(original);

			var idFilter = Builders<BsonDocument>.Filter.Eq("_id", original["_id"]);
			collection.UpdateOne(idFilter, updateDefinition);

			var result = collection.Find(idFilter).FirstOrDefault();
			result.Remove("_id");

			return result;
		}

		private BsonDocument RenderDefinition<TEntity>(UpdateDefinition<TEntity> definition)
		{
			var serializerRegistry = BsonSerializer.SerializerRegistry;
			var documentSerializer = serializerRegistry.GetSerializer<TEntity>();
			return definition.Render(documentSerializer, serializerRegistry);
		}

		[TestMethod]
		public void CreateFromDiffWithNothingToUpdate()
		{
			var documentA = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 20},
				{"Name", "Peter"},
				{"RegisteredDate", new DateTime(2017, 10, 1)},
				{"IsActive", true},
				{"RelatedIds", new int[] { 1, 3, 5, 7 }}
			});

			var documentB = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 20},
				{"Name", "Peter"},
				{"RegisteredDate", new DateTime(2017, 10, 1)},
				{"IsActive", true},
				{"RelatedIds", new int[] { 1, 3, 5, 7 }}
			});

			var expectedResult = new BsonDocument();

			var rawResult = UpdateDefinitionHelper.CreateFromDiff<BsonDocument>(documentA, documentB);
			var renderedResult = RenderDefinition(rawResult);

			Assert.AreEqual(expectedResult, renderedResult);
		}

		[TestMethod]
		public void CreateFromDiffWithUpdatedRootProperties()
		{
			var documentA = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 20},
				{"Name", "Peter"},
				{"RegisteredDate", new DateTime(2017, 10, 1)},
				{"IsActive", true},
				{"RelatedIds", new int[] { 1, 3, 5, 7 }},
				{"Status", "Active"},
				{"Description", null}
			});

			var documentB = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 17},
				{"Name", "Sam"},
				{"RegisteredDate", new DateTime(2018, 3, 10)},
				{"IsActive", false},
				{"RelatedIds", new int[] { 2, 4, 6, 8}},
				{"Status", null},
				{"Description", "Sam I am!"}
			});

			var expectedResult = new BsonDocument(new Dictionary<string, object>
			{
				{
					"$set", new Dictionary<string, object>
					{
						{"Age", 17},
						{"Name", "Sam"},
						{"RegisteredDate", new DateTime(2018, 3, 10)},
						{"IsActive", false},
						{"RelatedIds.0", 2},
						{"RelatedIds.1", 4},
						{"RelatedIds.2", 6},
						{"RelatedIds.3", 8},
						{"Status", null},
						{"Description", "Sam I am!"}
					}
				}
			});

			var rawResult = UpdateDefinitionHelper.CreateFromDiff<BsonDocument>(documentA, documentB);
			var renderedResult = RenderDefinition(rawResult);

			Assert.AreEqual(expectedResult, renderedResult);
			Assert.AreEqual(documentB, PerformUpdateAgainstServer(documentA, rawResult));
		}

		[TestMethod]
		public void CreateFromDiffWithNestedDocumentsToUpdate()
		{
			var documentA = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 20},
				{"Name", "Peter"},
				{
					"Address", new Dictionary<string, object>
					{
						{"Street", "My Test Road"},
						{"Suburb", "Test Suburb"}
					}
				}
			});

			var documentB = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 20},
				{"Name", "Peter"},
				{
					"Address", new Dictionary<string, object>
					{
						{"Street", "My Test Road"},
						{"Suburb", "Other Suburb"}
					}
				}
			});

			var expectedResult = new BsonDocument(new Dictionary<string, object>
			{
				{
					"$set", new Dictionary<string, object>
					{
						{"Address.Suburb", "Other Suburb"}
					}
				}
			});

			var rawResult = UpdateDefinitionHelper.CreateFromDiff<BsonDocument>(documentA, documentB);
			var renderedResult = RenderDefinition(rawResult);

			Assert.AreEqual(expectedResult, renderedResult);
			Assert.AreEqual(documentB, PerformUpdateAgainstServer(documentA, rawResult));
		}

		[TestMethod]
		public void CreateFromDiffWithNewProperties()
		{
			var documentA = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 20},
				{"Name", "Peter"}
			});

			var documentB = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 20},
				{"Name", "Peter"},
				{
					"Address", new Dictionary<string, object>
					{
						{"Street", "My Test Road"},
						{"Suburb", "Other Suburb"}
					}
				},
				{"RegisteredDate", new DateTime(2018, 3, 10)}
			});

			var expectedResult = new BsonDocument(new Dictionary<string, object>
			{
				{
					"$set", new Dictionary<string, object>
					{

						{
							"Address", new Dictionary<string, object>
							{
								{"Street", "My Test Road"},
								{"Suburb", "Other Suburb"}
							}
						},
						{"RegisteredDate", new DateTime(2018, 3, 10)}
					}
				}
			});

			var rawResult = UpdateDefinitionHelper.CreateFromDiff<BsonDocument>(documentA, documentB);
			var renderedResult = RenderDefinition(rawResult);

			Assert.AreEqual(expectedResult, renderedResult);
			Assert.AreEqual(documentB, PerformUpdateAgainstServer(documentA, rawResult));
		}

		[TestMethod]
		public void CreateFromDiffWithRemovedProperties()
		{
			var documentA = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 20},
				{"Name", "Peter"},
				{"RegisteredDate", new DateTime(2018, 3, 10)},
				{
					"Address", new Dictionary<string, object>
					{
						{"Street", "My Test Road"},
						{"Suburb", "Other Suburb"}
					}
				}
			});

			var documentB = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 20},
				{"Name", "Peter"}
			});

			var expectedResult = new BsonDocument(new Dictionary<string, object>
			{
				{
					"$unset", new Dictionary<string, object>
					{
						{"RegisteredDate", 1},
						{"Address", 1}
					}
				}
			});

			var rawResult = UpdateDefinitionHelper.CreateFromDiff<BsonDocument>(documentA, documentB);
			var renderedResult = RenderDefinition(rawResult);

			Assert.AreEqual(expectedResult, renderedResult);
			Assert.AreEqual(documentB, PerformUpdateAgainstServer(documentA, rawResult));
		}

		[TestMethod]
		public void CreateFromDiffWithNewArrayItems()
		{
			var documentA = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 20},
				{"Name", "Peter"},
				{"RelatedIds", new int[] { 2, 4 }}
			});

			var documentB = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 20},
				{"Name", "Peter"},
				{"RelatedIds", new int[] { 2, 4, 6 }}
			});

			var expectedResult = new BsonDocument(new Dictionary<string, object>
			{
				{
					"$set", new Dictionary<string, object>
					{
						{"RelatedIds", new int[] { 2, 4, 6 }}
					}
				}
			});

			var rawResult = UpdateDefinitionHelper.CreateFromDiff<BsonDocument>(documentA, documentB);
			var renderedResult = RenderDefinition(rawResult);

			Assert.AreEqual(expectedResult, renderedResult);
			Assert.AreEqual(documentB, PerformUpdateAgainstServer(documentA, rawResult));
		}

		[TestMethod]
		public void CreateFromDiffWithRemovedArrayItems()
		{
			var documentA = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 20},
				{"Name", "Peter"},
				{"RelatedIds", new int[] { 2, 4, 6 }}
			});

			var documentB = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 20},
				{"Name", "Peter"},
				{"RelatedIds", new int[] { 2, 4 }}
			});

			var expectedResult = new BsonDocument(new Dictionary<string, object>
			{
				{
					"$set", new Dictionary<string, object>
					{
						{"RelatedIds", new int[] { 2, 4 }}
					}
				}
			});

			var rawResult = UpdateDefinitionHelper.CreateFromDiff<BsonDocument>(documentA, documentB);
			var renderedResult = RenderDefinition(rawResult);

			Assert.AreEqual(expectedResult, renderedResult);
			Assert.AreEqual(documentB, PerformUpdateAgainstServer(documentA, rawResult));
		}

		[TestMethod]
		public void CreateFromDiffWithUpdatedArrayItems()
		{
			var documentA = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 20},
				{"Name", "Peter"},
				{"RelatedIds", new int[] { 1, 3, 5 }}
			});

			var documentB = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 20},
				{"Name", "Peter"},
				{"RelatedIds", new int[] { 2, 4, 6 }}
			});

			var expectedResult = new BsonDocument(new Dictionary<string, object>
			{
				{
					"$set", new Dictionary<string, object>
					{
						{"RelatedIds.0", 2},
						{"RelatedIds.1", 4},
						{"RelatedIds.2", 6}
					}
				}
			});

			var rawResult = UpdateDefinitionHelper.CreateFromDiff<BsonDocument>(documentA, documentB);
			var renderedResult = RenderDefinition(rawResult);

			Assert.AreEqual(expectedResult, renderedResult);
			Assert.AreEqual(documentB, PerformUpdateAgainstServer(documentA, rawResult));
		}
	}
}
