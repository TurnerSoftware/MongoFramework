using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;
using MongoFramework.Tests.Models;

namespace MongoFramework.Tests {
	[TestClass]
	public class MongoDbUtilityTests {

		[TestMethod]
		public void GetMongoUrlFromConfigTest1() {
			var url = MongoDbUtility.GetMongoUrlFromConfig("MongoFrameworkTests");
			Assert.IsNotNull(url);
		}

		[TestMethod]
		public void GetMongoUrlFromConfigTest2() {
			var url = MongoDbUtility.GetMongoUrlFromConfig("MongoFrameworkTests");
			Assert.IsTrue(url.DatabaseName == "MongoFrameworkTests");
		}

		[TestMethod]
		public void GetMongoUrlFromConfigTest3() {
			for (var i = 0; i < 150; i++) {
				var url = MongoDbUtility.GetMongoUrlFromConfig(Guid.NewGuid().ToString());
				Assert.IsNull(url);
			}
		}

		[TestMethod]
		public void GetMongoUrlFromConfigTest4() {
			var url = MongoDbUtility.GetMongoUrlFromConfig(null);
			Assert.IsNull(url);
		}

		[TestMethod]
		public void GetDatabaseTest1() {
			var url = MongoDbUtility.GetMongoUrlFromConfig("MongoFrameworkTests");
			var db = MongoDbUtility.GetDatabase(url);
			Assert.IsNotNull(db);
		}

		[TestMethod]
		[ExpectedException(typeof(NullReferenceException), "NullReferenceException")]
		public void GetDatabaseTest2() {
			MongoDbUtility.GetDatabase(null);
		}

		[TestMethod]
		public void IsValidObjectIdTest1() {
			for (var i = 0; i < 12000; i++) {
				var s = Guid.NewGuid().ToString().Replace("-", string.Empty).Trim();
				Assert.IsFalse(MongoDbUtility.IsValidObjectId(s));
			}
		}

		[TestMethod]
		public void IsValidObjectIdTest2() {

			var connectionString = TestConfiguration.ConnectionString;
			var databaseName = TestConfiguration.GetDatabaseName();
			var db = new MongoDbContextTestContext(connectionString, databaseName);

			for (var i = 0; i < 150; i++) {
				var model = new MongoDbContextModel();
				db.ContextDbSet.Add(model);
				db.SaveChanges();

				Assert.IsTrue(MongoDbUtility.IsValidObjectId(model.Id));
			}
		}
	}
}