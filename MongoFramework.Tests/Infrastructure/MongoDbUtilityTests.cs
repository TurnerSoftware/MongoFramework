using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;
using System;
using System.Linq;

namespace MongoFramework.Tests.Infrastructure
{
	[TestClass]
	public class MongoDbUtilityTests : TestBase
	{
		public class MongoDbUtilityModel
		{
			public string Id { get; set; }
		}
#if !NETCOREAPP2_0
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
#endif

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException), "ArgumentNullException")]
		public void DatabaseFromNullUrl()
		{
			MongoDbUtility.GetDatabase((MongoDB.Driver.MongoUrl)null);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException), "ArgumentNullException")]
		public void DatabaseFromNullOptions()
		{
			MongoDbUtility.GetDatabase((IMongoDbContextOptions)null);
		}

		[TestMethod]
		public void ValidObjectId()
		{
			var database = TestConfiguration.GetDatabase();
			var entityContainer = new DbEntityCollection<MongoDbUtilityModel>();
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
