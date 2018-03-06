using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoFramework.Bson;

namespace MongoFramework.Tests.Bson
{
	[TestClass]
	public class HasDifferencesTests
	{
		[TestMethod]
		public void NullDocument()
		{
			var documentA = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 20},
				{"Name", "Peter"},
				{"RegisteredDate", new DateTime(2017, 10, 1)},
				{"IsActive", true}
			});

			Assert.IsTrue(BsonDiff.HasDifferences(documentA, null));
			Assert.IsTrue(BsonDiff.HasDifferences(null, documentA));
			Assert.IsFalse(BsonDiff.HasDifferences((BsonDocument)null, null));
		}

		[TestMethod]
		public void DocumentHasNoDifferences()
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

			Assert.IsFalse(BsonDiff.HasDifferences(documentA, documentB));
			Assert.IsFalse(BsonDiff.HasDifferences(documentB, documentA));
		}

		[TestMethod]
		public void DocumentHasDifferentBasicPropertyValue()
		{
			var documentA = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 20}
			});

			var documentB = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 30}
			});

			Assert.IsTrue(BsonDiff.HasDifferences(documentA, documentB));
			Assert.IsTrue(BsonDiff.HasDifferences(documentB, documentA));
		}

		[TestMethod]
		public void DocumentHasAdditionalProperties()
		{
			var documentA = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 20}
			});

			var documentB = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 20},
				{"Name", "Peter"}
			});

			Assert.IsTrue(BsonDiff.HasDifferences(documentA, documentB));
			Assert.IsTrue(BsonDiff.HasDifferences(documentB, documentA));
		}

		[TestMethod]
		public void DocumentHasMissingProperties()
		{
			var documentA = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 20},
				{"Name", "Peter"}
			});

			var documentB = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 20}
			});

			Assert.IsTrue(BsonDiff.HasDifferences(documentA, documentB));
			Assert.IsTrue(BsonDiff.HasDifferences(documentB, documentA));
		}

		[TestMethod]
		public void DocumentHasSamePropertyCountButDifferentNames()
		{
			var documentA = new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 20},
				{"Name", "Peter"}
			});

			var documentB = new BsonDocument(new Dictionary<string, object>
			{
				{"YearsOld", 20},
				{"Name", "Peter"}
			});

			Assert.IsTrue(BsonDiff.HasDifferences(documentA, documentB));
			Assert.IsTrue(BsonDiff.HasDifferences(documentB, documentA));
		}

		[TestMethod]
		public void NullArray()
		{
			var arrayA = new BsonArray(Enumerable.Range(1, 5));

			Assert.IsTrue(BsonDiff.HasDifferences(arrayA, null));
			Assert.IsTrue(BsonDiff.HasDifferences(null, arrayA));
			Assert.IsFalse(BsonDiff.HasDifferences((BsonArray)null, null));
		}

		[TestMethod]
		public void ArrayHasNoDifferences()
		{
			var arrayA = new BsonArray(Enumerable.Range(1, 5));
			var arrayB = new BsonArray(Enumerable.Range(1, 5));

			Assert.IsFalse(BsonDiff.HasDifferences(arrayA, arrayB));
			Assert.IsFalse(BsonDiff.HasDifferences(arrayB, arrayA));
		}

		[TestMethod]
		public void ArrayHasDifferentItems()
		{
			var arrayA = new BsonArray(Enumerable.Range(1, 5));
			var arrayB = new BsonArray(Enumerable.Range(1, 5).Reverse());

			Assert.IsTrue(BsonDiff.HasDifferences(arrayA, arrayB));
			Assert.IsTrue(BsonDiff.HasDifferences(arrayB, arrayA));
		}

		[TestMethod]
		public void ArrayHasAdditionalItems()
		{
			var arrayA = new BsonArray(Enumerable.Range(1, 5));
			var arrayB = new BsonArray(Enumerable.Range(1, 10));

			Assert.IsTrue(BsonDiff.HasDifferences(arrayA, arrayB));
			Assert.IsTrue(BsonDiff.HasDifferences(arrayB, arrayA));
		}

		[TestMethod]
		public void ArrayHasMissingItems()
		{
			var arrayA = new BsonArray(Enumerable.Range(1, 5));
			var arrayB = new BsonArray(Enumerable.Range(1, 2));

			Assert.IsTrue(BsonDiff.HasDifferences(arrayA, arrayB));
			Assert.IsTrue(BsonDiff.HasDifferences(arrayB, arrayA));
		}

		[TestMethod]
		public void NullValue()
		{
			var valueA = (BsonValue)new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 20},
				{"Name", "Peter"},
				{"RegisteredDate", new DateTime(2017, 10, 1)},
				{"IsActive", true}
			});

			Assert.IsTrue(BsonDiff.HasDifferences(valueA, null));
			Assert.IsTrue(BsonDiff.HasDifferences(null, valueA));
			Assert.IsFalse(BsonDiff.HasDifferences((BsonValue)null, null));
		}

		[TestMethod]
		public void MismatchedValueType()
		{
			var valueA = (BsonValue)new BsonDocument(new Dictionary<string, object>
			{
				{"Age", 20},
				{"Name", "Peter"},
				{"RegisteredDate", new DateTime(2017, 10, 1)},
				{"IsActive", true}
			});
			var valueB = BsonValue.Create(123);

			Assert.IsTrue(BsonDiff.HasDifferences(valueA, valueB));
			Assert.IsTrue(BsonDiff.HasDifferences(valueB, valueA));
		}

		[TestMethod]
		public void DocumentBsonValue()
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

			Assert.IsFalse(BsonDiff.HasDifferences((BsonValue)documentA, documentB));
			Assert.IsFalse(BsonDiff.HasDifferences((BsonValue)documentB, documentA));
		}

		[TestMethod]
		public void ArrayBsonValue()
		{
			var arrayA = new BsonArray(Enumerable.Range(1, 5));
			var arrayB = new BsonArray(Enumerable.Range(1, 5));

			Assert.IsFalse(BsonDiff.HasDifferences((BsonValue)arrayA, arrayB));
			Assert.IsFalse(BsonDiff.HasDifferences((BsonValue)arrayB, arrayA));
		}
	}
}