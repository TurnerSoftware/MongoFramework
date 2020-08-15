using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Linq;
using MongoFramework.Infrastructure.Mapping;
using System.Linq;

namespace MongoFramework.Tests.Infrastructure.Linq
{
	[TestClass]
	public class MongoFrameworkQueryableTests : TestBase
	{
		public class TestProcessor<TEntity> : ILinqProcessor<TEntity> where TEntity : class
		{
			public bool EntityProcessed { get; private set; }

			public void ProcessEntity(TEntity entity, IMongoDbConnection connection)
			{
				EntityProcessed = true;
			}
		}

		public class MongoFrameworkQueryableModel
		{
			public string Id { get; set; }
			public string Title { get; set; }
		}

		[TestMethod]
		public void EnumerateQueryable()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			var commandWriter = new CommandWriter<MongoFrameworkQueryableModel>(connection);
			var entity = new MongoFrameworkQueryableModel { Title = "EnumerateQueryable" };
			var entry = new EntityEntry(entity, EntityEntryState.Added);
			commandWriter.Write(new[] { EntityCommandBuilder<MongoFrameworkQueryableModel>.CreateCommand(entry) });

			foreach (var dbEntity in queryable)
			{
				Assert.AreEqual("EnumerateQueryable", dbEntity.Title);
			}
		}

		[TestMethod]
		public void EntityProcessorsFiresOnEnumerationOfTEntity()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			var processor = new TestProcessor<MongoFrameworkQueryableModel>();
			provider.EntityProcessors.Add(processor);

			var commandWriter = new CommandWriter<MongoFrameworkQueryableModel>(connection);
			var entity = new MongoFrameworkQueryableModel { Title = "EntityProcessorFireTest" };
			var entry = new EntityEntry(entity, EntityEntryState.Added);
			commandWriter.Write(new[] { EntityCommandBuilder<MongoFrameworkQueryableModel>.CreateCommand(entry) });

			foreach (var dbEntity in queryable)
			{
				//Do nothing
			}

			Assert.IsTrue(processor.EntityProcessed);
		}

		[TestMethod]
		public void EntityProcessorsNotFiredWhenNotTEntity()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			var processor = new TestProcessor<MongoFrameworkQueryableModel>();
			provider.EntityProcessors.Add(processor);

			var commandWriter = new CommandWriter<MongoFrameworkQueryableModel>(connection);
			var entity = new MongoFrameworkQueryableModel { Title = "EntityProcessorNoFireTest" };
			var entry = new EntityEntry(entity, EntityEntryState.Added);
			commandWriter.Write(new[] { EntityCommandBuilder<MongoFrameworkQueryableModel>.CreateCommand(entry) });

			foreach (var titles in queryable.Select(e => e.Title))
			{
				//Do nothing
			}

			Assert.IsFalse(processor.EntityProcessed);
		}

		[TestMethod]
		public void EntityProcessorsRunWhenToDictionaryIsUsed()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			var processor = new TestProcessor<MongoFrameworkQueryableModel>();
			provider.EntityProcessors.Add(processor);

			var commandWriter = new CommandWriter<MongoFrameworkQueryableModel>(connection);
			var entity = new MongoFrameworkQueryableModel { Title = "EntityProcessorsRunWithToDictionaryTest" };
			var entry = new EntityEntry(entity, EntityEntryState.Added);
			commandWriter.Write(new[] { EntityCommandBuilder<MongoFrameworkQueryableModel>.CreateCommand(entry) });

			var result = queryable.ToDictionary(m => m.Id);
			Assert.AreEqual("EntityProcessorsRunWithToDictionaryTest", result.FirstOrDefault().Value.Title);
			Assert.IsTrue(processor.EntityProcessed);
		}
	}
}
