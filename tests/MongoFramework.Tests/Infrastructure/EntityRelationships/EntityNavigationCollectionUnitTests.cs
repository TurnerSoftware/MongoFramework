using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Mapping;
using System;

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
			var foreignProperty = EntityMapping.GetOrCreateDefinition(typeof(ObjectIdIdModel)).GetIdProperty();
			var collection = new EntityNavigationCollection<StringIdModel>(foreignProperty);
			collection.AddForeignId(null);
		}

		[TestMethod]
		public void AddForeignIdWithRightType()
		{
			var foreignProperty = EntityMapping.GetOrCreateDefinition(typeof(ObjectIdIdModel)).GetIdProperty();
			var collection = new EntityNavigationCollection<StringIdModel>(foreignProperty);
			collection.AddForeignId("12345678");
			Assert.AreEqual(1, collection.UnloadedCount);
		}

		[TestMethod, ExpectedException(typeof(InvalidOperationException))]
		public void AddForeignIdWithWrongType()
		{
			var foreignProperty = EntityMapping.GetOrCreateDefinition(typeof(ObjectIdIdModel)).GetIdProperty();
			var collection = new EntityNavigationCollection<StringIdModel>(foreignProperty);
			collection.AddForeignId(ObjectId.GenerateNewId());
		}

		[TestMethod]
		public void AddMultipleForeignIds()
		{
			var foreignProperty = EntityMapping.GetOrCreateDefinition(typeof(ObjectIdIdModel)).GetIdProperty();
			var collection = new EntityNavigationCollection<ObjectIdIdModel>(foreignProperty);
			collection.AddForeignIds(new object[] { ObjectId.GenerateNewId(), ObjectId.GenerateNewId() });
			Assert.AreEqual(2, collection.UnloadedCount);
		}
	}
}
