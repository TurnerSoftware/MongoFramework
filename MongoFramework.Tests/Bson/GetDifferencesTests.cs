using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoFramework.Bson;

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
				{"Comment", ""},
				{"Moderator", false},
				{"ModerationDate", null},
				{"Level", 5}
			});

			var documentB = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 30},
				{"Name", "Sam"},
				{"RegisteredDate", new DateTime(2017, 10, 1)},
				{"IsActive", true},
				{"Comment", ""},
				{"Moderator", true},
				{"ModerationDate", new DateTime(2017, 10, 5)},
				{"Level", 5}
			});

			var result = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 30},
				{"Name", "Sam"},
				{"Moderator", true},
				{"ModerationDate", new DateTime(2017, 10, 5)}
			});

			Assert.AreEqual(result, BsonDiff.GetDifferences(documentA, documentB).Difference);
		}
	}
}