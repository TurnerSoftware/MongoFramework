using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Linq;
using MongoFramework.Linq;
using MongoFramework.Tests.Models;
using System;
using System.Linq;

namespace MongoFramework.Tests.Linq
{
	[TestClass]
	public class LinqExtensionsTests
	{
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

			var dbEntityWriter = new DbEntityWriter<WhereIdMatchesGuidModel>(database);
			var entityCollection = new DbEntityCollection<WhereIdMatchesGuidModel>
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

			var dbEntityWriter = new DbEntityWriter<WhereIdMatchesObjectIdModel>(database);
			var entityCollection = new DbEntityCollection<WhereIdMatchesObjectIdModel>
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

			var dbEntityWriter = new DbEntityWriter<WhereIdMatchesStringModel>(database);
			var entityCollection = new DbEntityCollection<WhereIdMatchesStringModel>
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

		[TestMethod]
		public void WhereIdMatchesMixedIdTypes()
		{
			var database = TestConfiguration.GetDatabase();

			var dbEntityWriter = new DbEntityWriter<WhereIdMatchesStringModel>(database);
			var entityCollection = new DbEntityCollection<WhereIdMatchesStringModel>
			{
				new WhereIdMatchesStringModel { Description = "1" },
				new WhereIdMatchesStringModel { Description = "2" }
			};
			dbEntityWriter.Write(entityCollection);

			var dbEntityWriterObjectId = new DbEntityWriter<WhereIdMatchesObjectIdModel>(database);
			var entityCollectionObjectId = new DbEntityCollection<WhereIdMatchesObjectIdModel>
			{
				new WhereIdMatchesObjectIdModel { Description = "1" },
				new WhereIdMatchesObjectIdModel { Description = "2" }
			};
			dbEntityWriterObjectId.Write(entityCollectionObjectId);

			var dbEntityWriterGuid = new DbEntityWriter<WhereIdMatchesGuidModel>(database);
			var entityCollectionGuid = new DbEntityCollection<WhereIdMatchesGuidModel>
			{
				new WhereIdMatchesGuidModel { Description = "1" },
				new WhereIdMatchesGuidModel { Description = "2" }
			};
			dbEntityWriterGuid.Write(entityCollectionGuid);

			var collection = TestConfiguration.GetDatabase().GetCollection<WhereIdMatchesStringModel>("WhereIdMatchesStringModel");
			var underlyingQueryable = collection.AsQueryable();
			var queryable = new MongoFrameworkQueryable<WhereIdMatchesStringModel, WhereIdMatchesStringModel>(underlyingQueryable);

			var entityIds = new object[] { entityCollection.FirstOrDefault().Id, entityCollectionObjectId.FirstOrDefault().Id, entityCollectionGuid.FirstOrDefault().Id };

			var idMatchQueryable = LinqExtensions.WhereIdMatches(queryable, entityIds);

			Assert.AreEqual(1, idMatchQueryable.Count());
			Assert.IsTrue(idMatchQueryable.ToList().All(e => e.Id == (string)entityIds[0]));
		}
	}
}
