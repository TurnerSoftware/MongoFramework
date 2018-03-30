using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;
using MongoFramework.Tests.Models;
using System;
using System.Linq;

namespace MongoFramework.Tests
{
	[TestClass]
	public class MongoDbUtilityTests
	{
		[TestMethod]
		public void UrlFromConfigFound()
		{
			var url = MongoDbUtility.GetMongoUrlFromConfig("MongoFrameworkTests");
			Assert.IsNotNull(url);
		}

		[TestMethod]
		public void UrlFromConfigMissing()
		{
			var url = MongoDbUtility.GetMongoUrlFromConfig("ThisConnectionStringDoesntExist");
			Assert.IsNull(url);
		}

		[TestMethod]
		public void DatabaseFromUrl()
		{
			var url = MongoDbUtility.GetMongoUrlFromConfig("MongoFrameworkTests");
			var database = MongoDbUtility.GetDatabase(url);
			Assert.IsNotNull(database);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException), "ArgumentNullException")]
		public void DatabaseFromInvalidUrl()
		{
			MongoDbUtility.GetDatabase(null);
		}

		[TestMethod]
		public void ValidObjectId()
		{
			var database = TestConfiguration.GetDatabase();
			var entityContainer = new DbEntityContainer<MongoDbUtilityModel>();
			var writer = new DbEntityWriter<MongoDbUtilityModel>(database);

			var entity = new MongoDbUtilityModel();
			entityContainer.Update(entity, DbEntityEntryState.Added);
			writer.Write(entityContainer);

			Assert.IsTrue(MongoDbUtility.IsValidObjectId(entity.Id));
		}

		[TestMethod]
		public void InvalidObjectId()
		{
			Assert.IsFalse(MongoDbUtility.IsValidObjectId(string.Empty));
			Assert.IsFalse(MongoDbUtility.IsValidObjectId("0"));
			Assert.IsFalse(MongoDbUtility.IsValidObjectId("a"));
			Assert.IsFalse(MongoDbUtility.IsValidObjectId("0123456789ABCDEFGHIJKLMN"));
			Assert.IsFalse(MongoDbUtility.IsValidObjectId(null));
		}
	}
}
