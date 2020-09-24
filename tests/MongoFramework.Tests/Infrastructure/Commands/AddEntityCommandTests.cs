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
	public class AddEntityCommandTests : TestBase
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
		public void AddEntity()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);

			var entity = new TestModel
			{
				Title = "AddEntityCommandTests.AddEntity"
			};

			context.CommandStaging.Add(new AddEntityCommand<TestModel>(new EntityEntry(entity, EntityEntryState.Added)));
			context.SaveChanges();

			Assert.IsNotNull(entity.Id);
		}

		[TestMethod, ExpectedException(typeof(ValidationException))]
		public void ValidationExceptionOnInvalidModel()
		{
			var entity = new TestValidationModel { };
			var command = new AddEntityCommand<TestValidationModel>(new EntityEntry(entity, EntityEntryState.Added));
			command.GetModel(WriteModelOptions.Default).FirstOrDefault();
		}
	}
}
