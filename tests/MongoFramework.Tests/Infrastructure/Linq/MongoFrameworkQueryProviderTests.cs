using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Linq;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Tests.Infrastructure.Linq
{
	[TestClass]
	public class MongoFrameworkQueryProviderTests : TestBase
	{
		public class MongoFrameworkQueryableModel
		{
			public string Id { get; set; }
			public string Title { get; set; }
		}

		[TestMethod]
		public async Task EnumerateQueryableAsync()
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
		public async Task FirstOrDefaultAsyncQuery()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "FirstOrDefaultAsyncQuery" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = await queryable.AsAsyncEnumerable().FirstOrDefaultAsync();
			Assert.AreEqual("FirstOrDefaultAsyncQuery", result.Title);
		}
	}
}
