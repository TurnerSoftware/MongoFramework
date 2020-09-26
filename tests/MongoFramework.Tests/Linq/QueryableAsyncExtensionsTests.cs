using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Linq;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Linq;

namespace MongoFramework.Tests.Linq
{
	[TestClass]
	public class QueryableAsyncExtensionsTests : TestBase
	{
		public class QueryableAsyncModel
		{
			public string Id { get; set; }
			public string Title { get; set; }
			public DateTime Date { get; set; }
			public int IntNumber { get; set; }
		}

		[TestMethod]
		public async Task AsyncEnumeration()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "EnumerateQueryableAsync" }, EntityEntryState.Added);
			context.SaveChanges();

			await foreach (var entity in queryable.AsAsyncEnumerable())
			{
				Assert.AreEqual("EnumerateQueryableAsync", entity.Title);
			}
		}

		[TestMethod]
		public async Task ToArrayAsync()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "ToArrayAsync" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.ToArrayAsync();
			Assert.AreEqual(1, result.Length);
			Assert.AreEqual("ToArrayAsync", result[0].Title);
		}

		[TestMethod]
		public async Task ToListAsync()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "ToListAsync" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.ToListAsync();
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual("ToListAsync", result[0].Title);
		}

		[TestMethod]
		public async Task FirstAsync_NoValue()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncModel));

			var connection = TestConfiguration.GetConnection();
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncModel>(provider);

			await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await queryable.FirstAsync());
		}
		[TestMethod]
		public async Task FirstAsync_HasValue()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "FirstAsync_HasValue.1" }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "FirstAsync_HasValue.2" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.FirstAsync();
			Assert.AreEqual("FirstAsync_HasValue.1", result.Title);
		}
		[TestMethod]
		public async Task FirstAsync_WithPredicate()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "FirstAsync_WithPredicate.1" }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "FirstAsync_WithPredicate.2" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.FirstAsync(e => e.Title == "FirstAsync_WithPredicate.2");
			Assert.AreEqual("FirstAsync_WithPredicate.2", result.Title);
		}

		[TestMethod]
		public async Task FirstOrDefaultAsync_NoValue()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncModel));

			var connection = TestConfiguration.GetConnection();
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncModel>(provider);

			Assert.IsNull(await queryable.FirstOrDefaultAsync());
		}
		[TestMethod]
		public async Task FirstOrDefaultAsync_HasValue()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "FirstOrDefaultAsync_HasValue.1" }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "FirstOrDefaultAsync_HasValue.2" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.FirstOrDefaultAsync();
			Assert.AreEqual("FirstOrDefaultAsync_HasValue.1", result.Title);
		}
		[TestMethod]
		public async Task FirstOrDefaultAsync_WithPredicate()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "FirstOrDefaultAsync_WithPredicate.1" }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "FirstOrDefaultAsync_WithPredicate.2" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.FirstOrDefaultAsync(e => e.Title == "FirstOrDefaultAsync_WithPredicate.2");
			Assert.AreEqual("FirstOrDefaultAsync_WithPredicate.2", result.Title);
		}

		[TestMethod]
		public async Task SingleAsync_NoValue()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncModel));

			var connection = TestConfiguration.GetConnection();
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncModel>(provider);

			await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await queryable.SingleAsync());
		}
		[TestMethod]
		public async Task SingleAsync_HasValue()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "SingleAsync_HasValue.1" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.SingleAsync();
			Assert.AreEqual("SingleAsync_HasValue.1", result.Title);
		}
		[TestMethod]
		public async Task SingleAsync_HasMoreThanOneValue()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "SingleAsync_HasMoreThanOneValue.1" }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "SingleAsync_HasMoreThanOneValue.2" }, EntityEntryState.Added);
			context.SaveChanges();

			await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await queryable.SingleOrDefaultAsync());
		}
		[TestMethod]
		public async Task SingleAsync_WithPredicate()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "SingleAsync_WithPredicate.1" }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "SingleAsync_WithPredicate.2" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.SingleAsync(e => e.Title == "SingleAsync_WithPredicate.2");
			Assert.AreEqual("SingleAsync_WithPredicate.2", result.Title);
		}

		[TestMethod]
		public async Task SingleOrDefaultAsync_NoValue()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncModel));

			var connection = TestConfiguration.GetConnection();
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncModel>(provider);

			Assert.IsNull(await queryable.SingleOrDefaultAsync());
		}
		[TestMethod]
		public async Task SingleOrDefaultAsync_HasValue()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "SingleOrDefaultAsync_HasValue.1" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.SingleOrDefaultAsync();
			Assert.AreEqual("SingleOrDefaultAsync_HasValue.1", result.Title);
		}
		[TestMethod]
		public async Task SingleOrDefaultAsync_HasMoreThanOneValue()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "SingleOrDefaultAsync_HasMoreThanOneValue.1" }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "SingleOrDefaultAsync_HasMoreThanOneValue.2" }, EntityEntryState.Added);
			context.SaveChanges();

			await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await queryable.SingleOrDefaultAsync());
		}
		[TestMethod]
		public async Task SingleOrDefaultAsync_WithPredicate()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "SingleOrDefaultAsync_WithPredicate.1" }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "SingleOrDefaultAsync_WithPredicate.2" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.SingleOrDefaultAsync(e => e.Title == "SingleOrDefaultAsync_WithPredicate.2");
			Assert.AreEqual("SingleOrDefaultAsync_WithPredicate.2", result.Title);
		}

		[TestMethod]
		public async Task CountAsync_NoValues()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncModel>(provider);

			context.SaveChanges();

			var result = await queryable.CountAsync();
			Assert.AreEqual(0, result);
		}
		[TestMethod]
		public async Task CountAsync_HasValues()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "CountAsync_HasValues.1" }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "CountAsync_HasValues.2" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.CountAsync();
			Assert.AreEqual(2, result);
		}
		[TestMethod]
		public async Task CountAsync_WithPredicate()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "CountAsync_WithPredicate.1" }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "CountAsync_WithPredicate.2" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.CountAsync(e => e.Title == "CountAsync_WithPredicate.2");
			Assert.AreEqual(1, result);
		}

		[TestMethod]
		public async Task MaxAsync_NoValues()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncModel>(provider);

			context.SaveChanges();

			await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await queryable.Select(e => e.IntNumber).MaxAsync());
		}
		[TestMethod]
		public async Task MaxAsync_HasValues_Number()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "MaxAsync_HasValues_Number.1", IntNumber = 5 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "MaxAsync_HasValues_Number.2", IntNumber = 7 }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.Select(e => e.IntNumber).MaxAsync();
			Assert.AreEqual(7, result);
		}
		[TestMethod]
		public async Task MaxAsync_HasValues_Date()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "MaxAsync_HasValues_Date.1", Date = new DateTime(2020, 1, 10, 0, 0, 0, DateTimeKind.Utc) }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "MaxAsync_HasValues_Date.2", Date = new DateTime(2020, 3, 10, 0, 0, 0, DateTimeKind.Utc) }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.Select(e => e.Date).MaxAsync();
			Assert.AreEqual(new DateTime(2020, 3, 10, 0, 0, 0, DateTimeKind.Utc), result);
		}
		[TestMethod]
		public async Task MaxAsync_WithSelector()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "MaxAsync_WithSelector.1", IntNumber = 10 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "MaxAsync_WithSelector.2", IntNumber = 20 }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.MaxAsync(e => e.IntNumber);
			Assert.AreEqual(20, result);
		}

		[TestMethod]
		public async Task MinAsync_NoValues()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncModel>(provider);

			context.SaveChanges();

			await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await queryable.Select(e => e.IntNumber).MinAsync());
		}
		[TestMethod]
		public async Task MinAsync_HasValues_Number()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "MinAsync_HasValues_Number.1", IntNumber = 7 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "MinAsync_HasValues_Number.2", IntNumber = 5 }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.Select(e => e.IntNumber).MinAsync();
			Assert.AreEqual(5, result);
		}
		[TestMethod]
		public async Task MinAsync_HasValues_Date()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "MinAsync_HasValues_Date.1", Date = new DateTime(2020, 3, 10, 0, 0, 0, DateTimeKind.Utc) }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "MinAsync_HasValues_Date.2", Date = new DateTime(2020, 1, 10, 0, 0, 0, DateTimeKind.Utc) }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.Select(e => e.Date).MinAsync();
			Assert.AreEqual(new DateTime(2020, 1, 10, 0, 0, 0, DateTimeKind.Utc), result);
		}
		[TestMethod]
		public async Task MinAsync_WithSelector()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "MinAsync_WithSelector.1", IntNumber = 20 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "MinAsync_WithSelector.2", IntNumber = 10 }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.MinAsync(e => e.IntNumber);
			Assert.AreEqual(10, result);
		}

		[TestMethod]
		public async Task AnyAsync_NoValues()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncModel>(provider);

			var result = await queryable.AnyAsync();
			Assert.IsFalse(result);
		}
		[TestMethod]
		public async Task AnyAsync_HasValues()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "AnyAsync_HasValues.1" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.AnyAsync();
			Assert.IsTrue(result);
		}
		[TestMethod]
		public async Task AnyAsync_WithPredicate()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "AnyAsync_WithPredicate.1" }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncModel { Title = "AnyAsync_WithPredicate.2" }, EntityEntryState.Added);
			context.SaveChanges();

			var resultOne = await queryable.AnyAsync(e => e.Title == "AnyAsync_WithPredicate.2");
			Assert.IsTrue(resultOne);

			var resultTwo = await queryable.AnyAsync(e => e.Title == "AnyAsync_WithPredicate.3");
			Assert.IsFalse(resultTwo);
		}
	}
}
