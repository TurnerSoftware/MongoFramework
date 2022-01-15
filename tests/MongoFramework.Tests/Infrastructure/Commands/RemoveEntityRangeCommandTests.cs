using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Commands;

namespace MongoFramework.Tests.Infrastructure.Commands
{
	[TestClass]
	public class RemoveEntityRangeCommandTests : TestBase
	{
		public class TestModel
		{
			public string Id { get; set; }
			public string Title { get; set; }
			public DateTime DateTime { get; set; }
		}

		[TestMethod]
		public void RemoveEntities()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);

			var entities = new[]
			{
				new TestModel
				{
					Title = "RemoveEntityRangeCommandTests.RemoveEntities",
					DateTime = DateTime.UtcNow.AddDays(-1)
				},
				new TestModel
				{
					Title = "RemoveEntityRangeCommandTests.RemoveEntities",
					DateTime = DateTime.UtcNow.AddDays(1)
				}
			};

			context.CommandStaging.Add(new AddEntityCommand<TestModel>(new EntityEntry(entities[0], EntityEntryState.Added)));
			context.CommandStaging.Add(new AddEntityCommand<TestModel>(new EntityEntry(entities[1], EntityEntryState.Added)));
			context.SaveChanges();

			context.CommandStaging.Add(new RemoveEntityRangeCommand<TestModel>(e => e.DateTime < DateTime.UtcNow));
			context.SaveChanges();

			var removedEntity = context.Query<TestModel>().Where(e => e.Id == entities[0].Id).FirstOrDefault();
			Assert.IsNull(removedEntity);

			var nonRemovedEntity = context.Query<TestModel>().Where(e => e.Id == entities[1].Id).FirstOrDefault();
			Assert.IsNotNull(nonRemovedEntity);
		}
	}
}
