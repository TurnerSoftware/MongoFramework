﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoFramework.Infrastructure.EntityRelationships;
using System;
using System.Linq;

namespace MongoFramework.Tests.Infrastructure.EntityRelationships
{
	[TestClass]
	public class EntityNavigationCollectionUnitTests : TestBase
	{
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void NullForeignKeyThrowsException()
		{
			new EntityNavigationCollection<StringIdModel>(null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void AddForeignIdWithNull()
		{
			var collection = new EntityNavigationCollection<StringIdModel>("Id");
			collection.AddForeignId(null);
		}

		[TestMethod]
		public void AddForeignIdWithRightType()
		{
			var collection = new EntityNavigationCollection<StringIdModel>("Id");
			collection.AddForeignId("12345678");
			Assert.AreEqual(1, collection.UnloadedCount);
		}

		[TestMethod, ExpectedException(typeof(InvalidOperationException))]
		public void AddForeignIdWithWrongType()
		{
			var collection = new EntityNavigationCollection<StringIdModel>("Id");
			collection.AddForeignId(ObjectId.GenerateNewId());
		}

		[TestMethod]
		public void AddMultipleForeignIds()
		{
			var collection = new EntityNavigationCollection<ObjectIdIdModel>("Id");
			collection.AddForeignIds(new object[] { ObjectId.GenerateNewId(), ObjectId.GenerateNewId() });
			Assert.AreEqual(2, collection.UnloadedCount);
		}
	}
}
