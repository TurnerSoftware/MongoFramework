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
			var writer = new CommandWriter<TestModel>(connection);
			var reader = new EntityReader<TestModel>(connection);

			var entity = new TestModel
			{
				Title = "RemoveEntityByIdCommandTests.RemoveEntity"
			};

			writer.Write(new[]
			{
				new AddEntityCommand<TestModel>(new EntityEntry(entity, EntityEntryState.Added))
			});

			writer.Write(new[]
			{
				new RemoveEntityByIdCommand<TestModel>(entity.Id)
			});

			var dbEntity = reader.AsQueryable().Where(e => e.Id == entity.Id).FirstOrDefault();
			Assert.IsNull(dbEntity);
		}
	}
}
