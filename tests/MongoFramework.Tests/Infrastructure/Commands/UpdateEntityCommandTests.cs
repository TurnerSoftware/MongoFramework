using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

		public class TestValidationModel
		{
			public string Id { get; set; }

			[Required]
			public string RequiredField { get; set; }
			public bool BooleanField { get; set; }
		}

		[TestMethod]
		public void UpdateEntity()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);

			var entity = new TestModel
			{
				Title = "UpdateEntityCommandTests.UpdateEntity"
			};

			context.CommandStaging.Add(new AddEntityCommand<TestModel>(new EntityEntry(entity, EntityEntryState.Added)));
			context.SaveChanges();

			var updatedEntity = new TestModel
			{
				Id = entity.Id,
				Title = "UpdateEntityCommandTests.UpdateEntity-Updated"
			};

			context.CommandStaging.Add(new UpdateEntityCommand<TestModel>(new EntityEntry(updatedEntity, EntityEntryState.Updated)));
			context.SaveChanges();

			var dbEntity = context.Query<TestModel>().Where(e => e.Id == entity.Id).FirstOrDefault();
			Assert.AreEqual("UpdateEntityCommandTests.UpdateEntity-Updated", dbEntity.Title);
		}

		[TestMethod, ExpectedException(typeof(ValidationException))]
		public void ValidationExceptionOnInvalidModel()
		{
			var entity = new TestValidationModel { };
			var command = new UpdateEntityCommand<TestValidationModel>(new EntityEntry(entity, EntityEntryState.Updated));
			command.GetModel(WriteModelOptions.Default).FirstOrDefault();
		}
	}
}
