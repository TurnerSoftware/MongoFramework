using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;

namespace MongoFramework.Tests.Infrastructure
{
	[TestClass]
	public class MongoDbUtilityTests : TestBase
	{
		public class MongoDbUtilityModel
		{
			public string Id { get; set; }
		}

		[TestMethod]
		public void ValidObjectId()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);

			var entity = new MongoDbUtilityModel();
			context.ChangeTracker.SetEntityState(entity, EntityEntryState.Added);
			context.SaveChanges();

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
