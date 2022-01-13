using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Attributes;

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
			public DateTime Date { get; set; }
		}

		class MongoDbContextTestContext : MongoDbContext
		{
			public MongoDbContextTestContext(IMongoDbConnection connection) : base(connection) { }
			public MongoDbSet<DbSetModel> DbSet { get; set; }
			[BucketSetOptions(5, nameof(BucketSubEntity.Date))]
			public MongoDbBucketSet<BucketGroupModel, BucketSubEntity> DbBucketSet { get; set; }
		}

		[TestMethod]
		public void ContextCreatedWithOptions()
		{
			using (var context = new MongoDbContextTestContext(TestConfiguration.GetConnection()))
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
			using (var context = new MongoDbContextTestContext(TestConfiguration.GetConnection()))
			{
				Assert.IsNotNull(context.DbSet);
				Assert.IsNotNull(context.DbBucketSet);
			}
		}

		[TestMethod]
		public void DbSetsHaveOptionsApplied()
		{
			using (var context = new MongoDbContextTestContext(TestConfiguration.GetConnection()))
			{
				Assert.AreEqual(5, context.DbBucketSet.BucketSize);
			}
		}

		[TestMethod]
		public void ContextSavesDbSets()
		{
			using (var context = new MongoDbContextTestContext(TestConfiguration.GetConnection()))
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
			using (var context = new MongoDbContextTestContext(TestConfiguration.GetConnection()))
			{
				context.DbSet.Add(new DbSetModel());
				Assert.IsFalse(context.DbSet.Any());
				await context.SaveChangesAsync().ConfigureAwait(false);
				Assert.IsTrue(context.DbSet.Any());
			}
		}

		[TestMethod]
		public void SuccessfullyAttachUntrackedEntity()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbSet<DbSetModel>(context);

			var model = new DbSetModel
			{
				Id = "abcd"
			};

			dbSet.Add(model);

			context.SaveChanges();

			ResetMongoDb();

			context = new MongoDbContext(connection);
			dbSet = new MongoDbSet<DbSetModel>(context);

			var result = dbSet.AsNoTracking().FirstOrDefault();

			context.Attach(result);

			Assert.AreEqual(MongoFramework.Infrastructure.EntityEntryState.NoChanges, context.ChangeTracker.GetEntry(result).State);
		}

		[TestMethod]
		public void SuccessfullyAttachUntrackedEntities()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbSet<DbSetModel>(context);

			var entities = new[] {
				new DbSetModel
				{
					Id = "abcd"
				},
				new DbSetModel
				{
					Id = "efgh"
				}
			};

			dbSet.AddRange(entities);

			context.SaveChanges();

			ResetMongoDb();

			context = new MongoDbContext(connection);
			dbSet = new MongoDbSet<DbSetModel>(context);

			var result = dbSet.AsNoTracking().ToList();

			context.AttachRange(result);

			Assert.AreEqual(MongoFramework.Infrastructure.EntityEntryState.NoChanges, context.ChangeTracker.GetEntry(result[0]).State);
			Assert.AreEqual(MongoFramework.Infrastructure.EntityEntryState.NoChanges, context.ChangeTracker.GetEntry(result[1]).State);
		}

	}
}