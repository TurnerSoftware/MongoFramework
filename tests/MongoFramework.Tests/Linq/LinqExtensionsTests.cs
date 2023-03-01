﻿using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoFramework.Attributes;
using MongoFramework.Infrastructure.Linq;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Linq;

namespace MongoFramework.Tests.Linq
{
	[TestClass]
	public class LinqExtensionsTests : TestBase
	{
		public class LinqExtensionsModel
		{
			public string Id { get; set; }
		}
		public class WhereIdMatchesInheritanceBaseModel
		{
			public string Id { get; set; }
		}
		public class WhereIdMatchesInheritanceDerivedModel : WhereIdMatchesInheritanceBaseModel
		{
			public string Description { get; set; }
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

		public class SearchTextModel
		{
			public string Id { get; set; }
			[Index(IndexType.Text)]
			public string Text { get; set; }
			public int MiscField { get; set; }
		}

		public class SearchTextTenantModel : IHaveTenantId
		{
			public string TenantId { get; set; }
			public string Id { get; set; }
			[Index(IndexType.Text)]
			public string Text { get; set; }
			public int MiscField { get; set; }
		}

		[TestMethod]
		public void ValidToQuery()
		{
			EntityMapping.RegisterType(typeof(LinqExtensionsModel));

			var connection = TestConfiguration.GetConnection();
			var provider = new MongoFrameworkQueryProvider<LinqExtensionsModel>(connection);
			var queryable = new MongoFrameworkQueryable<LinqExtensionsModel>(provider);
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
		public void WhereIdMatches_BaseTypeWithId()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbSet<WhereIdMatchesInheritanceDerivedModel>(context);

			var entityCollection = new[]
			{
				new WhereIdMatchesInheritanceDerivedModel { Description = "1" },
				new WhereIdMatchesInheritanceDerivedModel { Description = "2" },
				new WhereIdMatchesInheritanceDerivedModel { Description = "3" },
				new WhereIdMatchesInheritanceDerivedModel { Description = "4" }
			};
			dbSet.AddRange(entityCollection);
			context.SaveChanges();

			var provider = new MongoFrameworkQueryProvider<WhereIdMatchesInheritanceDerivedModel>(connection);
			var queryable = new MongoFrameworkQueryable<WhereIdMatchesInheritanceDerivedModel>(provider);

			var entityIds = entityCollection.Select(e => e.Id).Take(2);

			var idMatchQueryable = LinqExtensions.WhereIdMatches(queryable, entityIds);

			Assert.AreEqual(2, idMatchQueryable.Count());
			Assert.IsTrue(idMatchQueryable.ToList().All(e => entityIds.Contains(e.Id)));
		}

		[TestMethod]
		public void WhereIdMatchesGuids()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbSet<WhereIdMatchesGuidModel>(context);

			var entityCollection = new[]
			{
				new WhereIdMatchesGuidModel { Description = "1" },
				new WhereIdMatchesGuidModel { Description = "2" },
				new WhereIdMatchesGuidModel { Description = "3" },
				new WhereIdMatchesGuidModel { Description = "4" }
			};
			dbSet.AddRange(entityCollection);
			context.SaveChanges();

			var provider = new MongoFrameworkQueryProvider<WhereIdMatchesGuidModel>(connection);
			var queryable = new MongoFrameworkQueryable<WhereIdMatchesGuidModel>(provider);

			var entityIds = entityCollection.Select(e => e.Id).Take(2);

			var idMatchQueryable = LinqExtensions.WhereIdMatches(queryable, entityIds);

			Assert.AreEqual(2, idMatchQueryable.Count());
			Assert.IsTrue(idMatchQueryable.ToList().All(e => entityIds.Contains(e.Id)));
		}

		[TestMethod]
		public void WhereIdMatchesObjectIds()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbSet<WhereIdMatchesObjectIdModel>(context);

			var entityCollection = new[]
			{
				new WhereIdMatchesObjectIdModel { Description = "1" },
				new WhereIdMatchesObjectIdModel { Description = "2" },
				new WhereIdMatchesObjectIdModel { Description = "3" },
				new WhereIdMatchesObjectIdModel { Description = "4" }
			};
			dbSet.AddRange(entityCollection);
			context.SaveChanges();

			var provider = new MongoFrameworkQueryProvider<WhereIdMatchesObjectIdModel>(connection);
			var queryable = new MongoFrameworkQueryable<WhereIdMatchesObjectIdModel>(provider);

			var entityIds = entityCollection.Select(e => e.Id).Take(2);

			var idMatchQueryable = LinqExtensions.WhereIdMatches(queryable, entityIds);

			Assert.AreEqual(2, idMatchQueryable.Count());
			Assert.IsTrue(idMatchQueryable.ToList().All(e => entityIds.Contains(e.Id)));
		}

		[TestMethod]
		public void WhereIdMatchesStringIds()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbSet<WhereIdMatchesStringModel>(context);

			var entityCollection = new[]
			{
				new WhereIdMatchesStringModel { Description = "1" },
				new WhereIdMatchesStringModel { Description = "2" },
				new WhereIdMatchesStringModel { Description = "3" },
				new WhereIdMatchesStringModel { Description = "4" }
			};
			dbSet.AddRange(entityCollection);
			context.SaveChanges();

			var provider = new MongoFrameworkQueryProvider<WhereIdMatchesStringModel>(connection);
			var queryable = new MongoFrameworkQueryable<WhereIdMatchesStringModel>(provider);

			var entityIds = entityCollection.Select(e => e.Id).Take(2);

			var idMatchQueryable = LinqExtensions.WhereIdMatches(queryable, entityIds);

			Assert.AreEqual(2, idMatchQueryable.Count());
			Assert.IsTrue(idMatchQueryable.ToList().All(e => entityIds.Contains(e.Id)));
		}

		[TestMethod]
		public void SearchText()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbSet<SearchTextModel>(context);

			dbSet.AddRange(new SearchTextModel[]
			{
				new SearchTextModel { MiscField = 1, Text = "The quick brown fox jumps over the lazy dog." },
				new SearchTextModel { MiscField = 2, Text = "The five boxing wizards jump quickly." },
				new SearchTextModel { MiscField = 3, Text = "The quick brown fox jumps over the lazy dog." },
				new SearchTextModel { MiscField = 4, Text = "Jived fox nymph grabs quick waltz." },
			});
			context.SaveChanges();

			Assert.AreEqual(4, dbSet.SearchText("quick").Count());
			Assert.AreEqual(0, dbSet.SearchText("the").Count()); //Stop words aren't used in text indexes: https://docs.mongodb.com/manual/core/index-text/#supported-languages-and-stop-words
			Assert.AreEqual(2, dbSet.SearchText("dog").Count());
			Assert.AreEqual(1, dbSet.SearchText("jived").Count());

			Assert.AreEqual(1, dbSet.SearchText("quick").Where(e => e.MiscField == 3).Count());
		}

		[TestMethod]
		public void SearchTextTenant()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbTenantContext(connection, TestConfiguration.GetTenantId());
			var dbSet = new MongoDbTenantSet<SearchTextTenantModel>(context);

			var models = new SearchTextTenantModel[]
			{
				new SearchTextTenantModel { MiscField = 1, Text = "The quick brown fox jumps over the lazy dog." },
				new SearchTextTenantModel { MiscField = 2, Text = "The five boxing wizards jump quickly." },
				new SearchTextTenantModel { MiscField = 3, Text = "The quick brown fox jumps over the lazy dog." },
				new SearchTextTenantModel { MiscField = 4, Text = "Jived fox nymph grabs quick waltz." },
			};

			dbSet.AddRange(models);
			context.SaveChanges();

			//add some extras to ensure only our tenant's results are coming back.
			var context2 = new MongoDbTenantContext(connection, "second");
			var dbSet2 = new MongoDbTenantSet<SearchTextTenantModel>(context);
			dbSet2.AddRange(models);
			context2.SaveChanges();

			Assert.AreEqual(4, dbSet.SearchText("quick").Count());
			Assert.AreEqual(0, dbSet.SearchText("the").Count()); //Stop words aren't used in text indexes: https://docs.mongodb.com/manual/core/index-text/#supported-languages-and-stop-words
			Assert.AreEqual(2, dbSet.SearchText("dog").Count());
			Assert.AreEqual(1, dbSet.SearchText("jived").Count());

			Assert.AreEqual(1, dbSet.SearchText("quick").Where(e => e.MiscField == 3).Count());
		}
	}
}
