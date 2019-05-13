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
			var collection = connection.GetDatabase().GetCollection<MongoFrameworkQueryableModel>(nameof(MongoFrameworkQueryableModel));
			var underlyingQueryable = collection.AsQueryable();
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel, MongoFrameworkQueryableModel>(connection, underlyingQueryable);

			var entityCollection = new EntityCollection<MongoFrameworkQueryableModel>();
			var writerPipeline = new EntityWriterPipeline<MongoFrameworkQueryableModel>(connection);
			writerPipeline.AddCollection(entityCollection);
			entityCollection.Update(new MongoFrameworkQueryableModel { Title = "EnumerateQueryable" }, EntityEntryState.Added);
			writerPipeline.Write();

			foreach (var entity in queryable)
			{
				Assert.AreEqual("EnumerateQueryable", entity.Title);
			}
		}

		[TestMethod]
		public void EntityProcessorsFiresOnEnumerationOfTEntity()
		{
			EntityMapping.RegisterType(typeof(MongoFrameworkQueryableModel));

			var connection = TestConfiguration.GetConnection();
			var collection = connection.GetDatabase().GetCollection<MongoFrameworkQueryableModel>(nameof(MongoFrameworkQueryableModel));
			var underlyingQueryable = collection.AsQueryable();
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel, MongoFrameworkQueryableModel>(connection, underlyingQueryable);

			var processor = new TestProcessor<MongoFrameworkQueryableModel>();
			queryable.EntityProcessors.Add(processor);

			var entityCollection = new EntityCollection<MongoFrameworkQueryableModel>();
			var writerPipeline = new EntityWriterPipeline<MongoFrameworkQueryableModel>(connection);
			writerPipeline.AddCollection(entityCollection);
			entityCollection.Update(new MongoFrameworkQueryableModel { Title = "EntityProcessorFireTest" }, EntityEntryState.Added);
			writerPipeline.Write();

			foreach (var entity in queryable)
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
			var collection = connection.GetDatabase().GetCollection<MongoFrameworkQueryableModel>(nameof(MongoFrameworkQueryableModel));
			var underlyingQueryable = collection.AsQueryable();
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel, MongoFrameworkQueryableModel>(connection, underlyingQueryable);

			var processor = new TestProcessor<MongoFrameworkQueryableModel>();
			queryable.EntityProcessors.Add(processor);

			var entityCollection = new EntityCollection<MongoFrameworkQueryableModel>();
			var writerPipeline = new EntityWriterPipeline<MongoFrameworkQueryableModel>(connection);
			writerPipeline.AddCollection(entityCollection);
			entityCollection.Update(new MongoFrameworkQueryableModel { Title = "EntityProcessorNoFireTest" }, EntityEntryState.Added);
			writerPipeline.Write();

			foreach (var titles in queryable.Select(e => e.Title))
			{
				//Do nothing
			}

			Assert.IsFalse(processor.EntityProcessed);
		}
	}
}
