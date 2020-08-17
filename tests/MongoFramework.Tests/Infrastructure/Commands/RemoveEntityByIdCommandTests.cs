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
	public class RemoveEntityByIdCommandTests : TestBase
	{
		public class TestModel
		{
			public string Id { get; set; }
			public string Title { get; set; }
		}

		[TestMethod]
		public void RemoveEntity()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);

			var entity = new TestModel
			{
				Title = "RemoveEntityByIdCommandTests.RemoveEntity"
			};

			context.CommandStaging.Add(new AddEntityCommand<TestModel>(new EntityEntry(entity, EntityEntryState.Added)));
			context.SaveChanges();

			context.CommandStaging.Add(new RemoveEntityByIdCommand<TestModel>(entity.Id));
			context.SaveChanges();

			var dbEntity = context.Query<TestModel>().Where(e => e.Id == entity.Id).FirstOrDefault();
			Assert.IsNull(dbEntity);
		}
	}
}
