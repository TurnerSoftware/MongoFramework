using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace MongoFramework.Tests
{
	[TestClass]
	public class MongoDbContextTests : TestBase
	{
		public class MongoDbContextModel
		{
			public string Id { get; set; }
		}

		public class MongoDbContextTestContext : MongoDbContext
		{
			public MongoDbContextTestContext(IMongoDbContextOptions options) : base(options) { }
			public MongoDbContextTestContext(string connectionString, string databaseName) : base(connectionString, databaseName) { }
			public MongoDbSet<MongoDbContextModel> ContextDbSet { get; set; }
		}

		[TestMethod]
		public void ContextCreatedWithOptions()
		{
			var options = new MongoDbContextOptions
			{
				ConnectionString = TestConfiguration.ConnectionString,
				Database = TestConfiguration.GetDatabaseName()
			};

			using (var context = new MongoDbContextTestContext(options))
			{
				context.ContextDbSet.Add(new MongoDbContextModel());
				Assert.IsFalse(context.ContextDbSet.Any());
				context.SaveChanges();
				Assert.IsTrue(context.ContextDbSet.Any());
			}
		}

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