using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Attributes;
using System.Linq;
using System.Threading.Tasks;

namespace MongoFramework.Tests
{
	[TestClass]
	public class MongoDbContextTests : TestBase
	{
		class DbSetModel
		{
			public string Id { get; set; }
		}
		class BucketGroupModel
		{
			public string Name { get; set; }
		}
		class BucketSubEntity
		{
			public string Label { get; set; }
		}

		class MongoDbContextTestContext : MongoDbContext
		{
			public MongoDbContextTestContext(IMongoDbConnection options) : base(options) { }
			public MongoDbContextTestContext(string connectionString, string databaseName) : base(connectionString, databaseName) { }
			public MongoDbSet<DbSetModel> DbSet { get; set; }
			[BucketSetOptions(5)]
			public MongoDbBucketSet<BucketGroupModel, BucketSubEntity> DbBucketSet { get; set; }
		}

		[TestMethod]
		public void ContextCreatedWithOptions()
		{
			var options = new MongoDbConnection
			{
				ConnectionString = TestConfiguration.ConnectionString,
				Database = TestConfiguration.GetDatabaseName()
			};

			using (var context = new MongoDbContextTestContext(options))
			{
				context.DbSet.Add(new DbSetModel());
				Assert.IsFalse(context.DbSet.Any());
				context.SaveChanges();
				Assert.IsTrue(context.DbSet.Any());
			}
		}

		[TestMethod]
		public void ContextCreatesDbSets()
		{
			var connectionString = TestConfiguration.ConnectionString;
			var databaseName = TestConfiguration.GetDatabaseName();
			using (var context = new MongoDbContextTestContext(connectionString, databaseName))
			{
				Assert.IsNotNull(context.DbSet);
				Assert.IsNotNull(context.DbBucketSet);
			}
		}

		[TestMethod]
		public void DbSetsHaveOptionsApplied()
		{
			var connectionString = TestConfiguration.ConnectionString;
			var databaseName = TestConfiguration.GetDatabaseName();
			using (var context = new MongoDbContextTestContext(connectionString, databaseName))
			{
				Assert.AreEqual(5, context.DbBucketSet.BucketSize);
			}
		}

		[TestMethod]
		public void ContextSavesDbSets()
		{
			var connectionString = TestConfiguration.ConnectionString;
			var databaseName = TestConfiguration.GetDatabaseName();
			using (var context = new MongoDbContextTestContext(connectionString, databaseName))
			{
				context.DbSet.Add(new DbSetModel());
				Assert.IsFalse(context.DbSet.Any());
				context.SaveChanges();
				Assert.IsTrue(context.DbSet.Any());
			}
		}

		[TestMethod]
		public async Task ContextSavesDbSetsAsync()
		{
			var connectionString = TestConfiguration.ConnectionString;
			var databaseName = TestConfiguration.GetDatabaseName();
			using (var context = new MongoDbContextTestContext(connectionString, databaseName))
			{
				context.DbSet.Add(new DbSetModel());
				Assert.IsFalse(context.DbSet.Any());
				await context.SaveChangesAsync().ConfigureAwait(false);
				Assert.IsTrue(context.DbSet.Any());
			}
		}
	}
}