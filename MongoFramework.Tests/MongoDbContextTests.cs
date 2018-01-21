using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Tests
{
	public class MongoDbContextTestContext : MongoDbContext
	{
		public MongoDbContextTestContext(string connectionString, string databaseName) : base(connectionString, databaseName) { }

		public MongoDbSet<MongoDbContextModel> ContextDbSet { get; set; }
	}

	public class MongoDbContextModel
	{
		public string Id { get; set; }
	}

	[TestClass]
	public class MongoDbContextTests
	{
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
	}
}
