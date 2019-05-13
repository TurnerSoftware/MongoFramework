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
	public class AddEntityCommandTests : TestBase
	{
		public class TestModel
		{
			public string Id { get; set; }
			public string Title { get; set; }
		}

		[TestMethod]
		public void AddEntity()
		{
			var connection = TestConfiguration.GetConnection();
			var writer = new CommandWriter<TestModel>(connection);

			var entity = new TestModel
			{
				Title = "AddEntityCommandTests.AddEntity"
			};

			writer.Write(new[]
			{
				new AddEntityCommand<TestModel>(new EntityEntry<TestModel>(entity, EntityEntryState.Added))
			});

			Assert.IsNotNull(entity.Id);
		}
	}
}
