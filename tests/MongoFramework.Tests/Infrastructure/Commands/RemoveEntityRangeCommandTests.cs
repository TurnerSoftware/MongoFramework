using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
			var writer = new CommandWriter<TestModel>(connection);
			var reader = new EntityReader<TestModel>(connection);

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

			writer.Write(new[]
			{
				new AddEntityCommand<TestModel>(new EntityEntry(entities[0], EntityEntryState.Added)),
				new AddEntityCommand<TestModel>(new EntityEntry(entities[1], EntityEntryState.Added))
			});

			writer.Write(new[]
			{
				new RemoveEntityRangeCommand<TestModel>(e => e.DateTime < DateTime.UtcNow)
			});

			var removedEntity = reader.AsQueryable().Where(e => e.Id == entities[0].Id).FirstOrDefault();
			Assert.IsNull(removedEntity);

			var nonRemovedEntity = reader.AsQueryable().Where(e => e.Id == entities[1].Id).FirstOrDefault();
			Assert.IsNotNull(nonRemovedEntity);
		}
	}
}
