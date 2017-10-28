using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoFramework.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Tests
{
	[TestClass]
	public class BsonDiffTests
	{
		[TestMethod]
		public void DocumentHasNoDifferences()
		{
			var documentA = new BsonDocument(new Dictionary<string, object>
			{
				{ "Age", 20 },
				{ "Name", "Peter" },
				{ "RegisteredDate", new DateTime(2017, 10, 1) },
				{ "IsActive", true }
			});

			var documentB = new BsonDocument(new Dictionary<string, object>
			{
				{ "Age", 20 },
				{ "Name", "Peter" },
				{ "RegisteredDate", new DateTime(2017, 10, 1) },
				{ "IsActive", true }
			});

			Assert.IsFalse(BsonDiff.HasDifferences(documentA, documentB));
		}

		[TestMethod]
		public void DocumentHasDifferentBasicPropertyValue()
		{
			var documentA = new BsonDocument(new Dictionary<string, object>
			{
				{ "Age", 20 }
			});

			var documentB = new BsonDocument(new Dictionary<string, object>
			{
				{ "Age", 30 }
			});

			Assert.IsTrue(BsonDiff.HasDifferences(documentA, documentB));
		}

		[TestMethod]
		public void DocumentHasAdditionalProperties()
		{
			var documentA = new BsonDocument(new Dictionary<string, object>
			{
				{ "Age", 20 }
			});

			var documentB = new BsonDocument(new Dictionary<string, object>
			{
				{ "Age", 20 },
				{ "Name", "Peter" }
			});

			Assert.IsTrue(BsonDiff.HasDifferences(documentA, documentB));
		}

		[TestMethod]
		public void DocumentHasMissingProperties()
		{
			var documentA = new BsonDocument(new Dictionary<string, object>
			{
				{ "Age", 20 },
				{ "Name", "Peter" }
			});

			var documentB = new BsonDocument(new Dictionary<string, object>
			{
				{ "Age", 20 }
			});

			Assert.IsTrue(BsonDiff.HasDifferences(documentA, documentB));
		}

		[TestMethod]
		public void ArrayHasNoDifferences()
		{
			var arrayA = new BsonArray(Enumerable.Range(1, 5));
			var arrayB = new BsonArray(Enumerable.Range(1, 5));

			Assert.IsFalse(BsonDiff.HasDifferences(arrayA, arrayB));
		}

		[TestMethod]
		public void ArrayHasDifferentItems()
		{
			var arrayA = new BsonArray(Enumerable.Range(1, 5));
			var arrayB = new BsonArray(Enumerable.Range(1, 5).Reverse());

			Assert.IsTrue(BsonDiff.HasDifferences(arrayA, arrayB));
		}

		[TestMethod]
		public void ArrayHasAdditionalItems()
		{
			var arrayA = new BsonArray(Enumerable.Range(1, 5));
			var arrayB = new BsonArray(Enumerable.Range(1, 10));

			Assert.IsTrue(BsonDiff.HasDifferences(arrayA, arrayB));
		}

		[TestMethod]
		public void ArrayHasMissingItems()
		{
			var arrayA = new BsonArray(Enumerable.Range(1, 5));
			var arrayB = new BsonArray(Enumerable.Range(1, 2));

			Assert.IsTrue(BsonDiff.HasDifferences(arrayA, arrayB));
		}

		[TestMethod]
		public void GetDocumentDiffWithNoDifferences()
		{
			var documentA = new BsonDocument(new Dictionary<string, object>
			{
				{ "Age", 20 },
				{ "Name", "Peter" },
				{ "RegisteredDate", new DateTime(2017, 10, 1) },
				{ "IsActive", true }
			});

			var documentB = new BsonDocument(new Dictionary<string, object>
			{
				{ "Age", 20 },
				{ "Name", "Peter" },
				{ "RegisteredDate", new DateTime(2017, 10, 1) },
				{ "IsActive", true }
			});

			Assert.IsFalse(BsonDiff.GetDifferences(documentA, documentB).HasDifference);
		}

		[TestMethod]
		public void GetDocumentDiffWithPartialDifferences()
		{
			var documentA = new BsonDocument(new Dictionary<string, object>
			{
				{ "Age", 20 },
				{ "Name", "Peter" },
				{ "RegisteredDate", new DateTime(2017, 10, 1) },
				{ "IsActive", true },
				{ "Comment", "" },
				{ "Moderator", false },
				{ "ModerationDate", null },
				{ "Level", 5 }
			});

			var documentB = new BsonDocument(new Dictionary<string, object>
			{
				{ "Age", 30 },
				{ "Name", "Sam" },
				{ "RegisteredDate", new DateTime(2017, 10, 1) },
				{ "IsActive", true },
				{ "Comment", "" },
				{ "Moderator", true },
				{ "ModerationDate", new DateTime(2017, 10, 5) },
				{ "Level", 5 }
			});

			var result = new BsonDocument(new Dictionary<string, object>
			{
				{ "Age", 30 },
				{ "Name", "Sam" },
				{ "Moderator", true },
				{ "ModerationDate", new DateTime(2017, 10, 5) }
			});

			Assert.AreEqual(result, BsonDiff.GetDifferences(documentA, documentB).Difference);
		}
	}
}
