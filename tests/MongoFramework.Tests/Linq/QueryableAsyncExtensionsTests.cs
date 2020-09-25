using System;
using System.Collections.Generic;
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
		public class MongoFrameworkQueryableModel
		{
			public string Id { get; set; }
			public string Title { get; set; }
		}

		[TestMethod]
		public async Task AsyncEnumeration()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "EnumerateQueryableAsync" }, EntityEntryState.Added);
			context.SaveChanges();

			await foreach (var entity in queryable.AsAsyncEnumerable())
			{
				Assert.AreEqual("EnumerateQueryableAsync", entity.Title);
			}
		}

		[TestMethod]
		public async Task FirstAsync_NoValue()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await queryable.FirstAsync());
		}
		[TestMethod]
		public async Task FirstAsync_HasValue()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "FirstAsync_HasValue.1" }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "FirstAsync_HasValue.2" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.FirstAsync();
			Assert.AreEqual("FirstAsync_HasValue.1", result.Title);
		}
		[TestMethod]
		public async Task FirstAsync_WithPredicate()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "FirstAsync_WithPredicate.1" }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "FirstAsync_WithPredicate.2" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.FirstAsync(e => e.Title == "FirstAsync_WithPredicate.2");
			Assert.AreEqual("FirstAsync_WithPredicate.2", result.Title);
		}

		[TestMethod]
		public async Task FirstOrDefaultAsync_NoValue()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			Assert.IsNull(await queryable.FirstOrDefaultAsync());
		}
		[TestMethod]
		public async Task FirstOrDefaultAsync_HasValue()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "FirstOrDefaultAsync_HasValue.1" }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "FirstOrDefaultAsync_HasValue.2" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.FirstOrDefaultAsync();
			Assert.AreEqual("FirstOrDefaultAsync_HasValue.1", result.Title);
		}
		[TestMethod]
		public async Task FirstOrDefaultAsync_WithPredicate()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "FirstOrDefaultAsync_WithPredicate.1" }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "FirstOrDefaultAsync_WithPredicate.2" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.FirstOrDefaultAsync(e => e.Title == "FirstOrDefaultAsync_WithPredicate.2");
			Assert.AreEqual("FirstOrDefaultAsync_WithPredicate.2", result.Title);
		}

		[TestMethod]
		public async Task SingleAsync_NoValue()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await queryable.SingleAsync());
		}
		[TestMethod]
		public async Task SingleAsync_HasValue()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "SingleAsync_HasValue.1" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.SingleAsync();
			Assert.AreEqual("SingleAsync_HasValue.1", result.Title);
		}
		[TestMethod]
		public async Task SingleAsync_HasMoreThanOneValue()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "SingleAsync_HasMoreThanOneValue.1" }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "SingleAsync_HasMoreThanOneValue.2" }, EntityEntryState.Added);
			context.SaveChanges();

			await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await queryable.SingleOrDefaultAsync());
		}
		[TestMethod]
		public async Task SingleAsync_WithPredicate()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "SingleAsync_WithPredicate.1" }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "SingleAsync_WithPredicate.2" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.SingleAsync(e => e.Title == "SingleAsync_WithPredicate.2");
			Assert.AreEqual("SingleAsync_WithPredicate.2", result.Title);
		}

		[TestMethod]
		public async Task SingleOrDefaultAsync_NoValue()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			Assert.IsNull(await queryable.SingleOrDefaultAsync());
		}
		[TestMethod]
		public async Task SingleOrDefaultAsync_HasValue()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "SingleOrDefaultAsync_HasValue.1" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.SingleOrDefaultAsync();
			Assert.AreEqual("SingleOrDefaultAsync_HasValue.1", result.Title);
		}
		[TestMethod]
		public async Task SingleOrDefaultAsync_HasMoreThanOneValue()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "SingleOrDefaultAsync_HasMoreThanOneValue.1" }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "SingleOrDefaultAsync_HasMoreThanOneValue.2" }, EntityEntryState.Added);
			context.SaveChanges();

			await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await queryable.SingleOrDefaultAsync());
		}
		[TestMethod]
		public async Task SingleOrDefaultAsync_WithPredicate()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "SingleOrDefaultAsync_WithPredicate.1" }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "SingleOrDefaultAsync_WithPredicate.2" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.SingleOrDefaultAsync(e => e.Title == "SingleOrDefaultAsync_WithPredicate.2");
			Assert.AreEqual("SingleOrDefaultAsync_WithPredicate.2", result.Title);
		}
	}
}
