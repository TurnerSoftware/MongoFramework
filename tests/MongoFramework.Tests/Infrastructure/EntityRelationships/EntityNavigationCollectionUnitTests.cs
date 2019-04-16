using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoFramework.Infrastructure.EntityRelationships;
using System;

namespace MongoFramework.Tests.Infrastructure.EntityRelationships
{
	[TestClass]
	public class EntityNavigationCollectionUnitTests : TestBase
	{
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void NullForeignKeyThrowsException()
		{
			var connection = TestConfiguration.GetConnection();
			new EntityNavigationCollection<StringIdModel>(null, connection);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void NullConnectionThrowsException()
		{
			new EntityNavigationCollection<StringIdModel>("Id", null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void AddForeignIdWithNull()
		{
			var connection = TestConfiguration.GetConnection();
			var collection = new EntityNavigationCollection<StringIdModel>("Id", connection);
			collection.AddForeignId(null);
		}

		[TestMethod]
		public void AddForeignIdWithRightType()
		{
			var connection = TestConfiguration.GetConnection();
			var collection = new EntityNavigationCollection<StringIdModel>("Id", connection);
			collection.AddForeignId("12345678");
			Assert.AreEqual(1, collection.UnloadedCount);
		}

		[TestMethod, ExpectedException(typeof(InvalidOperationException))]
		public void AddForeignIdWithWrongType()
		{
			var connection = TestConfiguration.GetConnection();
			var collection = new EntityNavigationCollection<StringIdModel>("Id", connection);
			collection.AddForeignId(ObjectId.GenerateNewId());
		}

		[TestMethod]
		public void AddMultipleForeignIds()
		{
			var connection = TestConfiguration.GetConnection();
			var collection = new EntityNavigationCollection<ObjectIdIdModel>("Id", connection);
			collection.AddForeignIds(new object[] { ObjectId.GenerateNewId(), ObjectId.GenerateNewId() });
			Assert.AreEqual(2, collection.UnloadedCount);
		}
	}
}
