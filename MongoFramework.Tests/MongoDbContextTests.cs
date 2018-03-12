using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Tests.Models;

namespace MongoFramework.Tests
{
	[TestClass]
	public class MongoDbContextTests
	{
		[TestMethod]
		public void ContextCreatesDbSets()
		{
			var connectionString = TestConfiguration.ConnectionString;
			var databaseName = TestConfiguration.GetDatabaseName();
			using (var context = new MongoDbContextTestContext(connectionString, databaseName))
			{
				Assert.IsNotNull(context.ContextDbSet);
			}
		}

		[TestMethod]
		public void ContextSavesDbSets()
		{
			var connectionString = TestConfiguration.ConnectionString;
			var databaseName = TestConfiguration.GetDatabaseName();
			using (var context = new MongoDbContextTestContext(connectionString, databaseName))
			{
				context.ContextDbSet.Add(new MongoDbContextModel());
				Assert.IsFalse(context.ContextDbSet.Any());
				context.SaveChanges();
				Assert.IsTrue(context.ContextDbSet.Any());
			}
		}
	}
}