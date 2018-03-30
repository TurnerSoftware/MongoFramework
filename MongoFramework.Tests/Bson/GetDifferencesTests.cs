using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoFramework.Bson;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MongoFramework.Tests.Bson
{
	[TestClass]
	public class GetDifferencesTests
	{
		[TestMethod]
		public void GetDocumentDiffWithNoDifferences()
		{
			var documentA = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 20},
				{"Name", "Peter"},
				{"RegisteredDate", new DateTime(2017, 10, 1)},
				{"IsActive", true}
			});

			var documentB = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 20},
				{"Name", "Peter"},
				{"RegisteredDate", new DateTime(2017, 10, 1)},
				{"IsActive", true}
			});

			Assert.IsFalse(BsonDiff.GetDifferences(documentA, documentB).HasDifference);
		}

		[TestMethod]
		public void GetDocumentDiffWithPartialDifferences()
		{
			var documentA = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 20},
				{"Name", "Peter"},
				{"RegisteredDate", new DateTime(2017, 10, 1)},
				{"IsActive", true},
				{"Comment", string.Empty},
				{"Moderator", false},
				{"ModerationDate", null},
				{"Level", 5},
				{"ItemIDList", new int[] { 5, 10, 17 }},
				{
					"NestedDetails",
					new Dictionary<string, object>
					{
						{"ID", 23}
					}
				}
			});

			var documentB = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 30},
				{"Name", "Sam"},
				{"RegisteredDate", new DateTime(2017, 10, 1)},
				{"IsActive", true},
				{"Comment", string.Empty},
				{"Moderator", true},
				{"ModerationDate", new DateTime(2017, 10, 5)},
				{"Level", 5},
				{"ItemIDList", new int[] { 5, 10, 17, 22 }},
				{
					"NestedDetails",
					new Dictionary<string, object>
					{
						{"ID", 24}
					}
				}
			});

			var result = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 30},
				{"Name", "Sam"},
				{"Moderator", true},
				{"ModerationDate", new DateTime(2017, 10, 5)},
				{
					"ItemIDList",
					new Dictionary<string, object>
					{
						{"3", 22}
					}
				},
				{
					"NestedDetails",
					new Dictionary<string, object>
					{
						{"ID", 24}
					}
				}
			});

			Assert.AreEqual(result, BsonDiff.GetDifferences(documentA, documentB).Difference);
		}

		[TestMethod]
		public void GetDocumentDiffWithNullDocuments()
		{
			var documentA = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 20},
				{"Name", "Michael"}
			});

			var resultWithSameValues = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 20},
				{"Name", "Michael"}
			});

			var resultWithClearedValues = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", BsonUndefined.Value},
				{"Name", BsonUndefined.Value}
			});

			Assert.AreEqual(resultWithClearedValues, BsonDiff.GetDifferences(documentA, null).Difference);
			Assert.AreEqual(resultWithSameValues, BsonDiff.GetDifferences(null, documentA).Difference);
		}

		[TestMethod]
		public void GetBsonArrayDiffWithNoDifferences()
		{
			var arrayA = new BsonArray(Enumerable.Range(1, 5));
			var arrayB = new BsonArray(Enumerable.Range(1, 5));

			Assert.AreEqual(null, BsonDiff.GetDifferences(arrayA, arrayB).Difference);
			Assert.AreEqual(null, BsonDiff.GetDifferences(arrayB, arrayA).Difference);
		}

		[TestMethod]
		public void GetBsonArrayDiffWithNullArrays()
		{
			var arrayA = new BsonArray(Enumerable.Range(1, 5));
			var resultWithSameValues = new BsonDocument(new Dictionary<string, object>
			{
				{"0",1},
				{"1",2},
				{"2",3},
				{"3",4},
				{"4",5}
			});
			var resultWithClearedValues = new BsonDocument(new Dictionary<string, object>
			{
				{"0",BsonUndefined.Value},
				{"1",BsonUndefined.Value},
				{"2",BsonUndefined.Value},
				{"3",BsonUndefined.Value},
				{"4",BsonUndefined.Value}
			});

			Assert.AreEqual(resultWithClearedValues, BsonDiff.GetDifferences(arrayA, null).Difference);
			Assert.AreEqual(resultWithSameValues, BsonDiff.GetDifferences(null, arrayA).Difference);
			Assert.AreEqual(null, BsonDiff.GetDifferences((BsonArray)null, null).Difference);
		}

		[TestMethod]
		public void GetBsonArrayDiffWithSameCountButDifferentValues()
		{
			var arrayA = new BsonArray(new int[] { 1, 2, 3, 4, 5, 6 });
			var arrayB = new BsonArray(new int[] { 1, 3, 5, 7, 9, 11 });
			var resultWithArrayBAsDefault = new BsonDocument(new Dictionary<string, object>
			{
				{"1",3},
				{"2",5},
				{"3",7},
				{"4",9},
				{"5",11}
			});
			var resultWithArrayAAsDefault = new BsonDocument(new Dictionary<string, object>
			{
				{"1",2},
				{"2",3},
				{"3",4},
				{"4",5},
				{"5",6}
			});

			Assert.AreEqual(resultWithArrayBAsDefault, BsonDiff.GetDifferences(arrayA, arrayB).Difference);
			Assert.AreEqual(resultWithArrayAAsDefault, BsonDiff.GetDifferences(arrayB, arrayA).Difference);
		}
	}
}