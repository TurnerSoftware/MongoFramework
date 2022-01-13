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
	public class QueryableAsyncExtensions_SumTests : TestBase
	{
		public class QueryableAsyncSumModel
		{
			public string Id { get; set; }
			public string Title { get; set; }
			public int Int32Number { get; set; }
			public decimal DecimalNumber { get; set; }
			public double DoubleNumber { get; set; }
			public float FloatNumber { get; set; }
			public long LongNumber { get; set; }
			public int? NullableInt32Number { get; set; }
			public decimal? NullableDecimalNumber { get; set; }
			public double? NullableDoubleNumber { get; set; }
			public float? NullableFloatNumber { get; set; }
			public long? NullableLongNumber { get; set; }
		}

		[TestMethod]
		public async Task SumAsync_Int32_NoValue()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncSumModel));

			var connection = TestConfiguration.GetConnection();
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncSumModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncSumModel>(provider);

			var result = await queryable.Select(e => e.Int32Number).SumAsync();
			Assert.AreEqual(0, result);
		}
		[TestMethod]
		public async Task SumAsync_Int32_HasValue()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncSumModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncSumModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncSumModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_Int32_HasValue.1", Int32Number = 5 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_Int32_HasValue.2", Int32Number = 9 }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.Select(e => e.Int32Number).SumAsync();
			Assert.AreEqual(14, result);
		}
		[TestMethod]
		public async Task SumAsync_Int32_WithSelector()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncSumModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncSumModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncSumModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_Int32_WithSelector.1", Int32Number = 7 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_Int32_WithSelector.2", Int32Number = 5 }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.SumAsync(e => e.Int32Number);
			Assert.AreEqual(12, result);
		}

		[TestMethod]
		public async Task SumAsync_NullableInt32_NoValue()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncSumModel));

			var connection = TestConfiguration.GetConnection();
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncSumModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncSumModel>(provider);

			var result = await queryable.Select(e => e.NullableInt32Number).SumAsync();
			Assert.IsNull(result);
		}
		[TestMethod]
		public async Task SumAsync_NullableInt32_HasValue()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncSumModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncSumModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncSumModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_NullableInt32_HasValue.1", NullableInt32Number = 5 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_NullableInt32_HasValue.2", NullableInt32Number = 9 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_NullableInt32_HasValue.3" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.Select(e => e.NullableInt32Number).SumAsync();
			Assert.AreEqual(14, result);
		}
		[TestMethod]
		public async Task SumAsync_NullableInt32_WithSelector()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncSumModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncSumModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncSumModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_NullableInt32_WithSelector.1", NullableInt32Number = 7 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_NullableInt32_WithSelector.2", NullableInt32Number = 5 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_NullableInt32_WithSelector.3" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.SumAsync(e => e.NullableInt32Number);
			Assert.AreEqual(12, result);
		}

		[TestMethod]
		public async Task SumAsync_Decimal_NoValue()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncSumModel));

			var connection = TestConfiguration.GetConnection();
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncSumModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncSumModel>(provider);

			var result = await queryable.Select(e => e.DecimalNumber).SumAsync();
			Assert.AreEqual(0, result);
		}
		[TestMethod]
		public async Task SumAsync_Decimal_HasValue()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncSumModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncSumModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncSumModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_Decimal_HasValue.1", DecimalNumber = 5 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_Decimal_HasValue.2", DecimalNumber = 9 }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.Select(e => e.DecimalNumber).SumAsync();
			Assert.AreEqual(14, result);
		}
		[TestMethod]
		public async Task SumAsync_Decimal_WithSelector()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncSumModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncSumModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncSumModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_Decimal_WithSelector.1", DecimalNumber = 7 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_Decimal_WithSelector.2", DecimalNumber = 5 }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.SumAsync(e => e.DecimalNumber);
			Assert.AreEqual(12, result);
		}

		[TestMethod]
		public async Task SumAsync_NullableDecimal_NoValue()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncSumModel));

			var connection = TestConfiguration.GetConnection();
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncSumModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncSumModel>(provider);

			var result = await queryable.Select(e => e.NullableDecimalNumber).SumAsync();
			Assert.IsNull(result);
		}
		[TestMethod]
		public async Task SumAsync_NullableDecimal_HasValue()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncSumModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncSumModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncSumModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_NullableDecimal_HasValue.1", NullableDecimalNumber = 5 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_NullableDecimal_HasValue.2", NullableDecimalNumber = 9 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_NullableDecimal_HasValue.3" }, EntityEntryState.Added);
			context.SaveChanges();

			var a = queryable.Select(e => e.NullableDecimalNumber).Sum();
			var result = await queryable.Select(e => e.NullableDecimalNumber).SumAsync();
			Assert.AreEqual(14, result);
		}
		[TestMethod]
		public async Task SumAsync_NullableDecimal_WithSelector()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncSumModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncSumModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncSumModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_NullableDecimal_WithSelector.1", NullableDecimalNumber = 7 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_NullableDecimal_WithSelector.2", NullableDecimalNumber = 5 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_NullableDecimal_WithSelector.3" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.SumAsync(e => e.NullableDecimalNumber);
			Assert.AreEqual(12, result);
		}

		[TestMethod]
		public async Task SumAsync_Double_NoValue()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncSumModel));

			var connection = TestConfiguration.GetConnection();
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncSumModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncSumModel>(provider);

			var result = await queryable.Select(e => e.DoubleNumber).SumAsync();
			Assert.AreEqual(0, result);
		}
		[TestMethod]
		public async Task SumAsync_Double_HasValue()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncSumModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncSumModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncSumModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_Double_HasValue.1", DoubleNumber = 5 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_Double_HasValue.2", DoubleNumber = 9 }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.Select(e => e.DoubleNumber).SumAsync();
			Assert.AreEqual(14, result);
		}
		[TestMethod]
		public async Task SumAsync_Double_WithSelector()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncSumModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncSumModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncSumModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_Double_WithSelector.1", DoubleNumber = 7 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_Double_WithSelector.2", DoubleNumber = 5 }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.SumAsync(e => e.DoubleNumber);
			Assert.AreEqual(12, result);
		}

		[TestMethod]
		public async Task SumAsync_NullableDouble_NoValue()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncSumModel));

			var connection = TestConfiguration.GetConnection();
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncSumModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncSumModel>(provider);

			var result = await queryable.Select(e => e.NullableDoubleNumber).SumAsync();
			Assert.IsNull(result);
		}
		[TestMethod]
		public async Task SumAsync_NullableDouble_HasValue()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncSumModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncSumModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncSumModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_NullableDouble_HasValue.1", NullableDoubleNumber = 5 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_NullableDouble_HasValue.2", NullableDoubleNumber = 9 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_NullableDouble_HasValue.3" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.Select(e => e.NullableDoubleNumber).SumAsync();
			Assert.AreEqual(14, result);
		}
		[TestMethod]
		public async Task SumAsync_NullableDouble_WithSelector()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncSumModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncSumModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncSumModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_NullableDouble_WithSelector.1", NullableDoubleNumber = 7 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_NullableDouble_WithSelector.2", NullableDoubleNumber = 5 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_NullableDouble_WithSelector.3" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.SumAsync(e => e.NullableDoubleNumber);
			Assert.AreEqual(12, result);
		}

		[TestMethod]
		public async Task SumAsync_Float_NoValue()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncSumModel));

			var connection = TestConfiguration.GetConnection();
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncSumModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncSumModel>(provider);

			var result = await queryable.Select(e => e.FloatNumber).SumAsync();
			Assert.AreEqual(0, result);
		}
		[TestMethod]
		public async Task SumAsync_Float_HasValue()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncSumModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncSumModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncSumModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_Float_HasValue.1", FloatNumber = 5 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_Float_HasValue.2", FloatNumber = 9 }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.Select(e => e.FloatNumber).SumAsync();
			Assert.AreEqual(14, result);
		}
		[TestMethod]
		public async Task SumAsync_Float_WithSelector()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncSumModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncSumModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncSumModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_Float_WithSelector.1", FloatNumber = 7 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_Float_WithSelector.2", FloatNumber = 5 }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.SumAsync(e => e.FloatNumber);
			Assert.AreEqual(12, result);
		}

		[TestMethod]
		public async Task SumAsync_NullableFloat_NoValue()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncSumModel));

			var connection = TestConfiguration.GetConnection();
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncSumModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncSumModel>(provider);

			var result = await queryable.Select(e => e.NullableFloatNumber).SumAsync();
			Assert.IsNull(result);
		}
		[TestMethod]
		public async Task SumAsync_NullableFloat_HasValue()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncSumModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncSumModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncSumModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_NullableFloat_HasValue.1", NullableFloatNumber = 5 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_NullableFloat_HasValue.2", NullableFloatNumber = 9 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_NullableFloat_HasValue.3" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.Select(e => e.NullableFloatNumber).SumAsync();
			Assert.AreEqual(14, result);
		}
		[TestMethod]
		public async Task SumAsync_NullableFloat_WithSelector()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncSumModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncSumModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncSumModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_NullableFloat_WithSelector.1", NullableFloatNumber = 7 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_NullableFloat_WithSelector.2", NullableFloatNumber = 5 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_NullableFloat_WithSelector.3" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.SumAsync(e => e.NullableFloatNumber);
			Assert.AreEqual(12, result);
		}

		[TestMethod]
		public async Task SumAsync_Long_NoValue()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncSumModel));

			var connection = TestConfiguration.GetConnection();
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncSumModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncSumModel>(provider);

			var result = await queryable.Select(e => e.LongNumber).SumAsync();
			Assert.AreEqual(0, result);
		}
		[TestMethod]
		public async Task SumAsync_Long_HasValue()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncSumModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncSumModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncSumModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_Long_HasValue.1", LongNumber = 5 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_Long_HasValue.2", LongNumber = 9 }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.Select(e => e.LongNumber).SumAsync();
			Assert.AreEqual(14, result);
		}
		[TestMethod]
		public async Task SumAsync_Long_WithSelector()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncSumModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncSumModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncSumModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_Long_WithSelector.1", LongNumber = 7 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_Long_WithSelector.2", LongNumber = 5 }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.SumAsync(e => e.LongNumber);
			Assert.AreEqual(12, result);
		}

		[TestMethod]
		public async Task SumAsync_NullableLong_NoValue()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncSumModel));

			var connection = TestConfiguration.GetConnection();
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncSumModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncSumModel>(provider);

			var result = await queryable.Select(e => e.NullableLongNumber).SumAsync();
			Assert.IsNull(result);
		}
		[TestMethod]
		public async Task SumAsync_NullableLong_HasValue()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncSumModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncSumModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncSumModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_NullableLong_HasValue.1", NullableLongNumber = 5 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_NullableLong_HasValue.2", NullableLongNumber = 9 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_NullableLong_HasValue.3" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.Select(e => e.NullableLongNumber).SumAsync();
			Assert.AreEqual(14, result);
		}
		[TestMethod]
		public async Task SumAsync_NullableLong_WithSelector()
		{
			EntityMapping.RegisterType(typeof(QueryableAsyncSumModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<QueryableAsyncSumModel>(connection);
			var queryable = new MongoFrameworkQueryable<QueryableAsyncSumModel>(provider);

			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_NullableLong_WithSelector.1", NullableLongNumber = 7 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_NullableLong_WithSelector.2", NullableLongNumber = 5 }, EntityEntryState.Added);
			context.ChangeTracker.SetEntityState(new QueryableAsyncSumModel { Title = "SumAsync_NullableLong_WithSelector.3" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.SumAsync(e => e.NullableLongNumber);
			Assert.AreEqual(12, result);
		}
	}
}
