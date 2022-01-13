using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Linq;

namespace MongoFramework.Tests.Linq
{
	[TestClass]
	public class QueryableAsyncExtensionsMultiTenantTests : TestBase
	{
		private class QueryableAsyncModel : IHaveTenantId
		{
			public string Id { get; set; }
			public string TenantId { get; set; }
			public string Title { get; set; }
			public DateTime Date { get; set; }
			public int IntNumber { get; set; }
		}

		private QueryableAsyncModel[] GetModels()
		{
			return new QueryableAsyncModel[]
			{
				new QueryableAsyncModel { Title = "ModelTitle.1", Date = new DateTime(2020, 1, 10, 0, 0, 0, DateTimeKind.Utc), IntNumber = 10 },
				new QueryableAsyncModel { Title = "ModelTitle.2", Date = new DateTime(2020, 2, 10, 0, 0, 0, DateTimeKind.Utc), IntNumber = 20 },
				new QueryableAsyncModel { Title = "ModelTitle.3", Date = new DateTime(2020, 3, 10, 0, 0, 0, DateTimeKind.Utc), IntNumber = 30 },
				new QueryableAsyncModel { Title = "ModelTitle.4", Date = new DateTime(2020, 4, 10, 0, 0, 0, DateTimeKind.Utc), IntNumber = 40 },
			};
		}

		private MongoDbTenantSet<QueryableAsyncModel> SetupTwoTenantsData(string tenantId)
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbTenantContext(connection, tenantId);
			var dbSet = new MongoDbTenantSet<QueryableAsyncModel>(context);

			var context2 = new MongoDbTenantContext(connection, tenantId + "-2");
			var dbSet2 = new MongoDbTenantSet<QueryableAsyncModel>(context2);

			dbSet.AddRange(GetModels());
			context.SaveChanges();

			dbSet2.AddRange(GetModels());
			context2.SaveChanges();

			return dbSet;
		}

		[TestMethod]
		public async Task AsyncEnumeration()
		{
			var tenantId = TestConfiguration.GetTenantId();
			var dbSet = SetupTwoTenantsData(tenantId);

			await foreach (var entity in dbSet.AsAsyncEnumerable())
			{
				Assert.AreEqual(tenantId, entity.TenantId);
			}
		}

		[TestMethod]
		public async Task ToArrayAsync()
		{
			var tenantId = TestConfiguration.GetTenantId();
			var dbSet = SetupTwoTenantsData(tenantId);

			var result = await dbSet.ToArrayAsync();
			Assert.AreEqual(tenantId, result[0].TenantId);
			Assert.AreEqual(tenantId, result[3].TenantId);
			Assert.AreEqual(4, result.Length);
			Assert.AreEqual("ModelTitle.1", result[0].Title);
			Assert.AreEqual("ModelTitle.4", result[3].Title);
		}

		[TestMethod]
		public async Task ToListAsync()
		{
			var tenantId = TestConfiguration.GetTenantId();
			var dbSet = SetupTwoTenantsData(tenantId);

			var result = await dbSet.ToListAsync();
			Assert.AreEqual(tenantId, result[0].TenantId);
			Assert.AreEqual(tenantId, result[3].TenantId);
			Assert.AreEqual(4, result.Count);
			Assert.AreEqual("ModelTitle.1", result[0].Title);
			Assert.AreEqual("ModelTitle.4", result[3].Title);
		}

		[TestMethod]
		public async Task FirstAsync_NoValue()
		{
			var tenantId = TestConfiguration.GetTenantId();
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbTenantContext(connection, tenantId);
			var dbSet = new MongoDbTenantSet<QueryableAsyncModel>(context);

			dbSet.AddRange(GetModels());
			context.SaveChanges();

			var context2 = new MongoDbTenantContext(connection, tenantId + "-2");
			var dbSet2 = new MongoDbTenantSet<QueryableAsyncModel>(context2);

			await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await dbSet2.FirstAsync());
		}
		[TestMethod]
		public async Task FirstAsync_HasValue()
		{
			var tenantId = TestConfiguration.GetTenantId();
			var dbSet = SetupTwoTenantsData(tenantId);

			var result = await dbSet.FirstAsync();
			Assert.AreEqual(tenantId, result.TenantId);
			Assert.AreEqual("ModelTitle.1", result.Title);
		}
		[TestMethod]
		public async Task FirstAsync_WithPredicate()
		{
			var tenantId = TestConfiguration.GetTenantId();
			var dbSet = SetupTwoTenantsData(tenantId);

			var result = await dbSet.FirstAsync(e => e.Title == "ModelTitle.2");
			Assert.AreEqual(tenantId, result.TenantId);
			Assert.AreEqual("ModelTitle.2", result.Title);
		}

		[TestMethod]
		public async Task FirstOrDefaultAsync_NoValue()
		{
			var tenantId = TestConfiguration.GetTenantId();
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbTenantContext(connection, tenantId);
			var dbSet = new MongoDbTenantSet<QueryableAsyncModel>(context);

			dbSet.AddRange(GetModels());
			context.SaveChanges();

			var context2 = new MongoDbTenantContext(connection, tenantId + "-2");
			var dbSet2 = new MongoDbTenantSet<QueryableAsyncModel>(context2);

			Assert.IsNull(await dbSet2.FirstOrDefaultAsync());
		}
		[TestMethod]
		public async Task FirstOrDefaultAsync_HasValue()
		{
			var tenantId = TestConfiguration.GetTenantId();
			var dbSet = SetupTwoTenantsData(tenantId);

			var result = await dbSet.FirstOrDefaultAsync();
			Assert.AreEqual(tenantId, result.TenantId);
			Assert.AreEqual("ModelTitle.1", result.Title);
		}
		[TestMethod]
		public async Task FirstOrDefaultAsync_WithPredicate()
		{
			var tenantId = TestConfiguration.GetTenantId();
			var dbSet = SetupTwoTenantsData(tenantId);

			var result = await dbSet.FirstOrDefaultAsync(e => e.Title == "ModelTitle.2");
			Assert.AreEqual(tenantId, result.TenantId);
			Assert.AreEqual("ModelTitle.2", result.Title);
		}

		[TestMethod]
		public async Task SingleAsync_NoValue()
		{
			var tenantId = TestConfiguration.GetTenantId();
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbTenantContext(connection, tenantId);
			var dbSet = new MongoDbTenantSet<QueryableAsyncModel>(context);

			dbSet.AddRange(GetModels());
			context.SaveChanges();

			var context2 = new MongoDbTenantContext(connection, tenantId + "-2");
			var dbSet2 = new MongoDbTenantSet<QueryableAsyncModel>(context2);

			await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await dbSet2.SingleAsync());
		}
		[TestMethod]
		public async Task SingleAsync_HasValue()
		{
			var tenantId = TestConfiguration.GetTenantId();
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbTenantContext(connection, tenantId);
			var dbSet = new MongoDbTenantSet<QueryableAsyncModel>(context);

			dbSet.AddRange(GetModels());
			context.SaveChanges();

			var context2 = new MongoDbTenantContext(connection, tenantId + "-2");
			var dbSet2 = new MongoDbTenantSet<QueryableAsyncModel>(context2);

			dbSet2.Add(new QueryableAsyncModel { Title = "SingleAsync_HasValue.1" });
			context2.SaveChanges();

			var result = await dbSet2.SingleAsync();
			Assert.AreEqual("SingleAsync_HasValue.1", result.Title);
		}
		[TestMethod]
		public async Task SingleAsync_HasMoreThanOneValue()
		{
			var tenantId = TestConfiguration.GetTenantId();
			var dbSet = SetupTwoTenantsData(tenantId);

			await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await dbSet.SingleOrDefaultAsync());
		}
		[TestMethod]
		public async Task SingleAsync_WithPredicate()
		{
			var tenantId = TestConfiguration.GetTenantId();
			var dbSet = SetupTwoTenantsData(tenantId);

			var result = await dbSet.SingleAsync(e => e.Title == "ModelTitle.2");
			Assert.AreEqual("ModelTitle.2", result.Title);
		}

		[TestMethod]
		public async Task SingleOrDefaultAsync_NoValue()
		{
			var tenantId = TestConfiguration.GetTenantId();
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbTenantContext(connection, tenantId);
			var dbSet = new MongoDbTenantSet<QueryableAsyncModel>(context);

			dbSet.AddRange(GetModels());
			context.SaveChanges();

			var context2 = new MongoDbTenantContext(connection, tenantId + "-2");
			var dbSet2 = new MongoDbTenantSet<QueryableAsyncModel>(context2);

			Assert.IsNull(await dbSet2.SingleOrDefaultAsync());
		}
		[TestMethod]
		public async Task SingleOrDefaultAsync_HasValue()
		{
			var tenantId = TestConfiguration.GetTenantId();
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbTenantContext(connection, tenantId);
			var dbSet = new MongoDbTenantSet<QueryableAsyncModel>(context);

			dbSet.AddRange(GetModels());
			context.SaveChanges();

			var context2 = new MongoDbTenantContext(connection, tenantId + "-2");
			var dbSet2 = new MongoDbTenantSet<QueryableAsyncModel>(context2);

			dbSet2.Add(new QueryableAsyncModel { Title = "SingleOrDefaultAsync_HasValue.1" });
			context2.SaveChanges();

			var result = await dbSet2.SingleOrDefaultAsync();
			Assert.AreEqual("SingleOrDefaultAsync_HasValue.1", result.Title);
		}
		[TestMethod]
		public async Task SingleOrDefaultAsync_HasMoreThanOneValue()
		{
			var tenantId = TestConfiguration.GetTenantId();
			var dbSet = SetupTwoTenantsData(tenantId);

			await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await dbSet.SingleOrDefaultAsync());
		}
		[TestMethod]
		public async Task SingleOrDefaultAsync_WithPredicate()
		{
			var tenantId = TestConfiguration.GetTenantId();
			var dbSet = SetupTwoTenantsData(tenantId);

			var result = await dbSet.SingleOrDefaultAsync(e => e.Title == "ModelTitle.2");
			Assert.AreEqual("ModelTitle.2", result.Title);
		}

		[TestMethod]
		public async Task CountAsync_NoValues()
		{
			var tenantId = TestConfiguration.GetTenantId();
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbTenantContext(connection, tenantId);
			var dbSet = new MongoDbTenantSet<QueryableAsyncModel>(context);

			dbSet.AddRange(GetModels());
			context.SaveChanges();

			var context2 = new MongoDbTenantContext(connection, tenantId + "-2");
			var dbSet2 = new MongoDbTenantSet<QueryableAsyncModel>(context2);

			var result = await dbSet2.CountAsync();
			Assert.AreEqual(0, result);
		}
		[TestMethod]
		public async Task CountAsync_HasValues()
		{
			var tenantId = TestConfiguration.GetTenantId();
			var dbSet = SetupTwoTenantsData(tenantId);

			var result = await dbSet.CountAsync();
			Assert.AreEqual(4, result);
		}
		[TestMethod]
		public async Task CountAsync_WithPredicate()
		{
			var tenantId = TestConfiguration.GetTenantId();
			var dbSet = SetupTwoTenantsData(tenantId);

			var result = await dbSet.CountAsync(e => e.Title == "ModelTitle.2");
			Assert.AreEqual(1, result);
		}

		[TestMethod]
		public async Task MaxAsync_NoValues()
		{
			var tenantId = TestConfiguration.GetTenantId();
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbTenantContext(connection, tenantId);
			var dbSet = new MongoDbTenantSet<QueryableAsyncModel>(context);

			dbSet.AddRange(GetModels());
			context.SaveChanges();

			var context2 = new MongoDbTenantContext(connection, tenantId + "-2");
			var dbSet2 = new MongoDbTenantSet<QueryableAsyncModel>(context2);

			await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await dbSet2.Select(e => e.IntNumber).MaxAsync());
		}
		[TestMethod]
		public async Task MaxAsync_HasValues_Number()
		{
			var tenantId = TestConfiguration.GetTenantId();
			var dbSet = SetupTwoTenantsData(tenantId);

			var result = await dbSet.Select(e => e.IntNumber).MaxAsync();
			Assert.AreEqual(40, result);
		}
		[TestMethod]
		public async Task MaxAsync_HasValues_Date()
		{
			var tenantId = TestConfiguration.GetTenantId();
			var dbSet = SetupTwoTenantsData(tenantId);

			var result = await dbSet.Select(e => e.Date).MaxAsync();
			Assert.AreEqual(new DateTime(2020, 4, 10, 0, 0, 0, DateTimeKind.Utc), result);
		}
		[TestMethod]
		public async Task MaxAsync_WithSelector()
		{
			var tenantId = TestConfiguration.GetTenantId();
			var dbSet = SetupTwoTenantsData(tenantId);

			var result = await dbSet.MaxAsync(e => e.IntNumber);
			Assert.AreEqual(40, result);
		}

		[TestMethod]
		public async Task MinAsync_NoValues()
		{
			var tenantId = TestConfiguration.GetTenantId();
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbTenantContext(connection, tenantId);
			var dbSet = new MongoDbTenantSet<QueryableAsyncModel>(context);

			dbSet.AddRange(GetModels());
			context.SaveChanges();

			var context2 = new MongoDbTenantContext(connection, tenantId + "-2");
			var dbSet2 = new MongoDbTenantSet<QueryableAsyncModel>(context2);

			await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await dbSet2.Select(e => e.IntNumber).MinAsync());
		}
		[TestMethod]
		public async Task MinAsync_HasValues_Number()
		{
			var tenantId = TestConfiguration.GetTenantId();
			var dbSet = SetupTwoTenantsData(tenantId);

			var result = await dbSet.Select(e => e.IntNumber).MinAsync();
			Assert.AreEqual(10, result);
		}
		[TestMethod]
		public async Task MinAsync_HasValues_Date()
		{
			var tenantId = TestConfiguration.GetTenantId();
			var dbSet = SetupTwoTenantsData(tenantId);

			var result = await dbSet.Select(e => e.Date).MinAsync();
			Assert.AreEqual(new DateTime(2020, 1, 10, 0, 0, 0, DateTimeKind.Utc), result);
		}
		[TestMethod]
		public async Task MinAsync_WithSelector()
		{
			var tenantId = TestConfiguration.GetTenantId();
			var dbSet = SetupTwoTenantsData(tenantId);

			var result = await dbSet.MinAsync(e => e.IntNumber);
			Assert.AreEqual(10, result);
		}

		[TestMethod]
		public async Task AnyAsync_NoValues()
		{
			var tenantId = TestConfiguration.GetTenantId();
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbTenantContext(connection, tenantId);
			var dbSet = new MongoDbTenantSet<QueryableAsyncModel>(context);

			dbSet.AddRange(GetModels());
			context.SaveChanges();

			var context2 = new MongoDbTenantContext(connection, tenantId + "-2");
			var dbSet2 = new MongoDbTenantSet<QueryableAsyncModel>(context2);

			var result = await dbSet2.AnyAsync();
			Assert.IsFalse(result);
		}
		[TestMethod]
		public async Task AnyAsync_HasValues()
		{
			var tenantId = TestConfiguration.GetTenantId();
			var dbSet = SetupTwoTenantsData(tenantId);

			var result = await dbSet.AnyAsync();
			Assert.IsTrue(result);
		}
		[TestMethod]
		public async Task AnyAsync_WithPredicate()
		{
			var tenantId = TestConfiguration.GetTenantId();
			var dbSet = SetupTwoTenantsData(tenantId);

			var result = await dbSet.AnyAsync(e => e.Title == "ModelTitle.2");
			Assert.IsTrue(result);
			result = await dbSet.AnyAsync(e => e.Title == "ModelTitle.5");
			Assert.IsFalse(result);
		}




	}
}
