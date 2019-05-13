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
	public class UpdateEntityCommandTests : TestBase
	{
		public class TestModel
		{
			public string Id { get; set; }
			public string Title { get; set; }
		}

		[TestMethod]
		public void UpdateEntity()
		{
			var connection = TestConfiguration.GetConnection();
			var writer = new CommandWriter<TestModel>(connection);
			var reader = new EntityReader<TestModel>(connection);

			var entity = new TestModel
			{
				Title = "UpdateEntityCommandTests.UpdateEntity"
			};

			writer.Write(new[]
			{
				new AddEntityCommand<TestModel>(new EntityEntry<TestModel>(entity, EntityEntryState.Added))
			});

			var updatedEntity = new TestModel
			{
				Id = entity.Id,
				Title = "UpdateEntityCommandTests.UpdateEntity-Updated"
			};

			writer.Write(new[]
			{
				new UpdateEntityCommand<TestModel>(new EntityEntry<TestModel>(updatedEntity, EntityEntryState.Updated))
			});

			var dbEntity = reader.AsQueryable().Where(e => e.Id == entity.Id).FirstOrDefault();
			Assert.AreEqual("UpdateEntityCommandTests.UpdateEntity-Updated", dbEntity.Title);
		}
	}
}
