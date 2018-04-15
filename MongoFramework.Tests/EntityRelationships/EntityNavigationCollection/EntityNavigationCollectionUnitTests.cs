using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoFramework.Infrastructure;

namespace MongoFramework.Tests.EntityRelationships.EntityNavigationCollection
{
	[TestClass]
	public class EntityNavigationCollectionUnitTests
	{
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void NullForeignKeyThrowsException()
		{
			new Infrastructure.EntityRelationships.EntityNavigationCollection<StringIdModel>(null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void AddForeignIdWithNull()
		{
			var collection = new Infrastructure.EntityRelationships.EntityNavigationCollection<StringIdModel>("Id");
			collection.AddForeignId(null);
		}

		[TestMethod]
		public void AddForeignIdWithRightType()
		{
			var collection = new Infrastructure.EntityRelationships.EntityNavigationCollection<StringIdModel>("Id");
			collection.AddForeignId("12345678");
			Assert.AreEqual(1, collection.UnloadedCount);
		}

		[TestMethod, ExpectedException(typeof(InvalidOperationException))]
		public void AddForeignIdWithWrongType()
		{
			var collection = new Infrastructure.EntityRelationships.EntityNavigationCollection<StringIdModel>("Id");
			collection.AddForeignId(ObjectId.GenerateNewId());
		}

		[TestMethod]
		public void AddMultipleForeignIds()
		{
			var collection = new Infrastructure.EntityRelationships.EntityNavigationCollection<ObjectIdIdModel>("Id");
			collection.AddForeignIds(new object[] { ObjectId.GenerateNewId(), ObjectId.GenerateNewId() });
			Assert.AreEqual(2, collection.UnloadedCount);
		}
	}
}
