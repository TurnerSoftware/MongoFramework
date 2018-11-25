using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Linq;
using MongoFramework.Linq;
using System;
using System.Linq;

namespace MongoFramework.Tests.Linq
{
	[TestClass]
	public class LinqExtensionsTests : TestBase
	{
		public class LinqExtensionsModel
		{
			public string Id { get; set; }
		}
		public class WhereIdMatchesGuidModel
		{
			public Guid Id { get; set; }
			public string Description { get; set; }
		}
		public class WhereIdMatchesObjectIdModel
		{
			public ObjectId Id { get; set; }
			public string Description { get; set; }
		}
		public class WhereIdMatchesStringModel
		{
			public string Id { get; set; }
			public string Description { get; set; }
		}

		[TestMethod]
		public void ValidToQuery()
		{
			var collection = TestConfiguration.GetDatabase().GetCollection<LinqExtensionsModel>("LinqExtensionsModel");
			var underlyingQueryable = collection.AsQueryable();
			var queryable = new MongoFrameworkQueryable<LinqExtensionsModel, LinqExtensionsModel>(underlyingQueryable);
			var result = LinqExtensions.ToQuery(queryable);

			Assert.AreEqual("db.LinqExtensionsModel.aggregate([])", result);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException), "ArgumentException")]
		public void InvalidToQuery()
		{
			LinqExtensions.ToQuery(null);
		}

		[TestMethod]
		public void WhereIdMatchesGuids()
		{
			var database = TestConfiguration.GetDatabase();

			var dbEntityWriter = new EntityWriter<WhereIdMatchesGuidModel>(database);
			var entityCollection = new EntityCollection<WhereIdMatchesGuidModel>
			{
				new WhereIdMatchesGuidModel { Description = "1" },
				new WhereIdMatchesGuidModel { Description = "2" },
				new WhereIdMatchesGuidModel { Description = "3" },
				new WhereIdMatchesGuidModel { Description = "4" }
			};
			dbEntityWriter.Write(entityCollection);

			var collection = TestConfiguration.GetDatabase().GetCollection<WhereIdMatchesGuidModel>("WhereIdMatchesGuidModel");
			var underlyingQueryable = collection.AsQueryable();
			var queryable = new MongoFrameworkQueryable<WhereIdMatchesGuidModel, WhereIdMatchesGuidModel>(underlyingQueryable);

			var entityIds = entityCollection.Select(e => e.Id).Take(2);

			var idMatchQueryable = LinqExtensions.WhereIdMatches(queryable, entityIds);

			Assert.AreEqual(2, idMatchQueryable.Count());
			Assert.IsTrue(idMatchQueryable.ToList().All(e => entityIds.Contains(e.Id)));
		}

		[TestMethod]
		public void WhereIdMatchesObjectIds()
		{
			var database = TestConfiguration.GetDatabase();

			var dbEntityWriter = new EntityWriter<WhereIdMatchesObjectIdModel>(database);
			var entityCollection = new EntityCollection<WhereIdMatchesObjectIdModel>
			{
				new WhereIdMatchesObjectIdModel { Description = "1" },
				new WhereIdMatchesObjectIdModel { Description = "2" },
				new WhereIdMatchesObjectIdModel { Description = "3" },
				new WhereIdMatchesObjectIdModel { Description = "4" }
			};
			dbEntityWriter.Write(entityCollection);

			var collection = TestConfiguration.GetDatabase().GetCollection<WhereIdMatchesObjectIdModel>("WhereIdMatchesObjectIdModel");
			var underlyingQueryable = collection.AsQueryable();
			var queryable = new MongoFrameworkQueryable<WhereIdMatchesObjectIdModel, WhereIdMatchesObjectIdModel>(underlyingQueryable);

			var entityIds = entityCollection.Select(e => e.Id).Take(2);

			var idMatchQueryable = LinqExtensions.WhereIdMatches(queryable, entityIds);

			Assert.AreEqual(2, idMatchQueryable.Count());
			Assert.IsTrue(idMatchQueryable.ToList().All(e => entityIds.Contains(e.Id)));
		}

		[TestMethod]
		public void WhereIdMatchesStringIds()
		{
			var database = TestConfiguration.GetDatabase();

			var dbEntityWriter = new EntityWriter<WhereIdMatchesStringModel>(database);
			var entityCollection = new EntityCollection<WhereIdMatchesStringModel>
			{
				new WhereIdMatchesStringModel { Description = "1" },
				new WhereIdMatchesStringModel { Description = "2" },
				new WhereIdMatchesStringModel { Description = "3" },
				new WhereIdMatchesStringModel { Description = "4" }
			};
			dbEntityWriter.Write(entityCollection);

			var collection = TestConfiguration.GetDatabase().GetCollection<WhereIdMatchesStringModel>("WhereIdMatchesStringModel");
			var underlyingQueryable = collection.AsQueryable();
			var queryable = new MongoFrameworkQueryable<WhereIdMatchesStringModel, WhereIdMatchesStringModel>(underlyingQueryable);

			var entityIds = entityCollection.Select(e => e.Id).Take(2);

			var idMatchQueryable = LinqExtensions.WhereIdMatches(queryable, entityIds);

			Assert.AreEqual(2, idMatchQueryable.Count());
			Assert.IsTrue(idMatchQueryable.ToList().All(e => entityIds.Contains(e.Id)));
		}
	}
}
