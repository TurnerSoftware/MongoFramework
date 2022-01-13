using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Linq;
using MongoFramework.Infrastructure.Mapping;

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
		public class MixedReadA
		{
			public string Id { get; set; }
			public string Description { get; set; }
		}
		public class MixedReadB : MixedReadA
		{
			public bool BIsForBoolean { get; set; }
		}

		[TestMethod]
		public void EnumerateQueryable()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "EnumerateQueryable" }, EntityEntryState.Added);
			context.SaveChanges();

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
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			var processor = new TestProcessor<MongoFrameworkQueryableModel>();
			provider.EntityProcessors.Add(processor);

			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "EntityProcessorFireTest" }, EntityEntryState.Added);
			context.SaveChanges();

			foreach (var dbEntity in queryable)
			{
				//Do nothing
			}

			Assert.IsTrue(processor.EntityProcessed);
		}

		[TestMethod]
		public void EntityProcessorsNotFiredWhenNotTEntity_Select()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			var processor = new TestProcessor<MongoFrameworkQueryableModel>();
			provider.EntityProcessors.Add(processor);

			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "EntityProcessorsNotFiredWhenNotTEntity_Select" }, EntityEntryState.Added);
			context.SaveChanges();

			foreach (var titles in queryable.Select(e => e.Title))
			{
				//Do nothing
			}

			Assert.IsFalse(processor.EntityProcessed);
		}

		[TestMethod]
		public void EntityProcessorsNotFiredWhenNotTEntity_Any()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			var processor = new TestProcessor<MongoFrameworkQueryableModel>();
			provider.EntityProcessors.Add(processor);

			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "EntityProcessorsNotFiredWhenNotTEntity_Any" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = queryable.Any(e => e.Title == "EntityProcessorsNotFiredWhenNotTEntity_Any");

			Assert.IsTrue(result);
			Assert.IsFalse(processor.EntityProcessed);
		}

		[TestMethod]
		public void EntityProcessorsRunWhenToDictionaryIsUsed()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<MongoFrameworkQueryableModel>(connection);
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel>(provider);

			var processor = new TestProcessor<MongoFrameworkQueryableModel>();
			provider.EntityProcessors.Add(processor);

			context.ChangeTracker.SetEntityState(new MongoFrameworkQueryableModel { Title = "EntityProcessorsRunWithToDictionaryTest" }, EntityEntryState.Added);
			context.SaveChanges();

			var result = queryable.ToDictionary(m => m.Id);
			Assert.AreEqual("EntityProcessorsRunWithToDictionaryTest", result.FirstOrDefault().Value.Title);
			Assert.IsTrue(processor.EntityProcessed);
		}

		[TestMethod]
		public void ReadMixedEntities()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var provider = new MongoFrameworkQueryProvider<MixedReadA>(connection);
			var queryable = new MongoFrameworkQueryable<MixedReadA>(provider);

			context.ChangeTracker.SetEntityState(new MixedReadA
			{
				Description = "MongoFrameworkQueryableTests.ReadMixedEntities"
			}, EntityEntryState.Added);

			context.ChangeTracker.SetEntityState<MixedReadA>(new MixedReadB
			{
				BIsForBoolean = true,
				Description = "MongoFrameworkQueryableTests.ReadMixedEntities"
			}, EntityEntryState.Added);

			context.SaveChanges();

			var readMixedEntitiesQuery = queryable.Where(e => e.Description == "MongoFrameworkQueryableTests.ReadMixedEntities");

			Assert.AreEqual(2, readMixedEntitiesQuery.Count());
			Assert.AreEqual(2, readMixedEntitiesQuery.OfType<MixedReadA>().Count());
			Assert.AreEqual(1, readMixedEntitiesQuery.OfType<MixedReadB>().Count());
		}
	}
}
