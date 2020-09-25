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
			public DateTime Date { get; set; }
			public int IntNumber { get; set; }
			public decimal DecimalNumber { get; set; }
			public double DoubleNumber { get; set; }
			public float FloatNumber { get; set; }
			public long LongNumber { get; set; }
			public int? NullableIntNumber { get; set; }
			public decimal? NullableDecimalNumber { get; set; }
			public double? NullableDoubleNumber { get; set; }
			public float? NullableFloatNumber { get; set; }
			public long? NullableLongNumber { get; set; }
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

		[TestMethod]
		public async Task CountAsync_NoValues()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			context.SaveChanges();

			var result = await queryable.CountAsync();
			Assert.AreEqual(0, result);
		}
		[TestMethod]
		public async Task CountAsync_HasValues()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "CountAsync_HasValues.1" }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "CountAsync_HasValues.2" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.CountAsync();
			Assert.AreEqual(2, result);
		}
		[TestMethod]
		public async Task CountAsync_WithPredicate()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "CountAsync_WithPredicate.1" }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "CountAsync_WithPredicate.2" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.CountAsync(e => e.Title == "CountAsync_WithPredicate.2");
			Assert.AreEqual(1, result);
		}

		[TestMethod]
		public async Task MaxAsync_NoValues()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			context.SaveChanges();

			await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await queryable.Select(e => e.IntNumber).MaxAsync());
		}
		[TestMethod]
		public async Task MaxAsync_HasValues_Number()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "MaxAsync_HasValues_Number.1", IntNumber = 5 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "MaxAsync_HasValues_Number.2", IntNumber = 7 }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.Select(e => e.IntNumber).MaxAsync();
			Assert.AreEqual(7, result);
		}
		[TestMethod]
		public async Task MaxAsync_HasValues_Date()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "MaxAsync_HasValues_Date.1", Date = new DateTime(2020, 1, 10, 0, 0, 0, DateTimeKind.Utc) }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "MaxAsync_HasValues_Date.2", Date = new DateTime(2020, 3, 10, 0, 0, 0, DateTimeKind.Utc) }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.Select(e => e.Date).MaxAsync();
			Assert.AreEqual(new DateTime(2020, 3, 10, 0, 0, 0, DateTimeKind.Utc), result);
		}
		[TestMethod]
		public async Task MaxAsync_WithSelector()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "MaxAsync_WithSelector.1", IntNumber = 10 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "MaxAsync_WithSelector.2", IntNumber = 20 }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.MaxAsync(e => e.IntNumber);
			Assert.AreEqual(20, result);
		}

		[TestMethod]
		public async Task MinAsync_NoValues()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			context.SaveChanges();

			await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await queryable.Select(e => e.IntNumber).MinAsync());
		}
		[TestMethod]
		public async Task MinAsync_HasValues_Number()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "MinAsync_HasValues_Number.1", IntNumber = 7 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "MinAsync_HasValues_Number.2", IntNumber = 5 }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.Select(e => e.IntNumber).MinAsync();
			Assert.AreEqual(5, result);
		}
		[TestMethod]
		public async Task MinAsync_HasValues_Date()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "MinAsync_HasValues_Date.1", Date = new DateTime(2020, 3, 10, 0, 0, 0, DateTimeKind.Utc) }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "MinAsync_HasValues_Date.2", Date = new DateTime(2020, 1, 10, 0, 0, 0, DateTimeKind.Utc) }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.Select(e => e.Date).MinAsync();
			Assert.AreEqual(new DateTime(2020, 1, 10, 0, 0, 0, DateTimeKind.Utc), result);
		}
		[TestMethod]
		public async Task MinAsync_WithSelector()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "MinAsync_WithSelector.1", IntNumber = 20 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "MinAsync_WithSelector.2", IntNumber = 10 }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.MinAsync(e => e.IntNumber);
			Assert.AreEqual(10, result);
		}
	}
}
