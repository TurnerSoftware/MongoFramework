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
		public async Task EnumerateQueryable()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			var entityCollection = new EntityCollection<MongoFrameworkQueryableModel>();
			var writerPipeline = new EntityWriterPipeline<MongoFrameworkQueryableModel>(connection);
			writerPipeline.AddCollection(entityCollection);
			entityCollection.Update(new MongoFrameworkQueryableModel { Title = "EnumerateQueryable" }, EntityEntryState.Added);
			writerPipeline.Write();

			await foreach (var entity in queryable.AsAsyncEnumerable())
			{
				Assert.AreEqual("EnumerateQueryable", entity.Title);
			}
		}

		[TestMethod]
		public async Task FirstOrDefaultAsyncQuery()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			var entityCollection = new EntityCollection<MongoFrameworkQueryableModel>();
			var writerPipeline = new EntityWriterPipeline<MongoFrameworkQueryableModel>(connection);
			writerPipeline.AddCollection(entityCollection);
			entityCollection.Update(new MongoFrameworkQueryableModel { Title = "FirstOrDefaultAsyncQuery" }, EntityEntryState.Added);
			writerPipeline.Write();

			var result = await queryable.AsAsyncEnumerable().FirstOrDefaultAsync();
			Assert.AreEqual("FirstOrDefaultAsyncQuery", result.Title);
		}
	}
}
