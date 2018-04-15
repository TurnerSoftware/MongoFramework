using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Tests.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MongoFramework.Tests
{
	[TestClass]
	public class MongoDbContextTests : DbTest
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

		[TestMethod]
		public async Task ContextSavesDbSetsAsync()
		{
			var connectionString = TestConfiguration.ConnectionString;
			var databaseName = TestConfiguration.GetDatabaseName();
			using (var context = new MongoDbContextTestContext(connectionString, databaseName))
			{
				context.ContextDbSet.Add(new MongoDbContextModel());
				Assert.IsFalse(context.ContextDbSet.Any());
				await context.SaveChangesAsync().ConfigureAwait(false);
				Assert.IsTrue(context.ContextDbSet.Any());
			}
		}
	}
}