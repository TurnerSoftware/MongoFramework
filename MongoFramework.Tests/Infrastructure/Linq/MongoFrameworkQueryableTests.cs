using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Linq;
using MongoFramework.Infrastructure.Mapping;
using System;
using System.Linq;

namespace MongoFramework.Tests.Infrastructure.Linq
{
	[TestClass]
	public class MongoFrameworkQueryableTests : TestBase
	{
		public class TestProcessor<TEntity> : ILinqProcessor<TEntity> where TEntity : class
		{
			public bool EntityProcessed { get; private set; }

			public void ProcessEntity(TEntity entity)
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
			var connection = TestConfiguration.GetConnection();
			//new EntityMapper<MongoFrameworkQueryableModel>();
			var collection = connection.GetDatabase().GetCollection<MongoFrameworkQueryableModel>(nameof(MongoFrameworkQueryableModel));
			var underlyingQueryable = collection.AsQueryable();
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel, MongoFrameworkQueryableModel>(connection, underlyingQueryable);

			var entityContainer = new EntityCollection<MongoFrameworkQueryableModel>(connection.GetEntityMapper(typeof(MongoFrameworkQueryableModel)));
			var writer = new EntityWriter<MongoFrameworkQueryableModel>(connection);
			entityContainer.Update(new MongoFrameworkQueryableModel { Title = "EnumerateQueryable" }, EntityEntryState.Added);
			writer.Write(entityContainer);

			foreach (var entity in queryable)
			{
				Assert.AreEqual("EnumerateQueryable", entity.Title);
			}
		}

		[TestMethod]
		public void EntityProcessorsFiresOnEnumerationOfTEntity()
		{
			var connection = TestConfiguration.GetConnection();
			//new EntityMapper<MongoFrameworkQueryableModel>();
			var collection = connection.GetDatabase().GetCollection<MongoFrameworkQueryableModel>(nameof(MongoFrameworkQueryableModel));
			var underlyingQueryable = collection.AsQueryable();
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel, MongoFrameworkQueryableModel>(connection, underlyingQueryable);

			var processor = new TestProcessor<MongoFrameworkQueryableModel>();
			queryable.EntityProcessors.Add(processor);

			var entityContainer = new EntityCollection<MongoFrameworkQueryableModel>(connection.GetEntityMapper(typeof(MongoFrameworkQueryableModel)));
			var writer = new EntityWriter<MongoFrameworkQueryableModel>(connection);
			entityContainer.Update(new MongoFrameworkQueryableModel { Title = "EntityProcessorFireTest" }, EntityEntryState.Added);
			writer.Write(entityContainer);

			foreach (var entity in queryable)
			{
				//Do nothing
			}

			Assert.IsTrue(processor.EntityProcessed);
		}

		[TestMethod]
		public void EntityProcessorsNotFiredWhenNotTEntity()
		{
			var connection = TestConfiguration.GetConnection();
			//new EntityMapper<MongoFrameworkQueryableModel>();
			var collection = connection.GetDatabase().GetCollection<MongoFrameworkQueryableModel>(nameof(MongoFrameworkQueryableModel));
			var underlyingQueryable = collection.AsQueryable();
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel, MongoFrameworkQueryableModel>(connection, underlyingQueryable);

			var processor = new TestProcessor<MongoFrameworkQueryableModel>();
			queryable.EntityProcessors.Add(processor);

			var entityContainer = new EntityCollection<MongoFrameworkQueryableModel>(connection.GetEntityMapper(typeof(MongoFrameworkQueryableModel)));
			var writer = new EntityWriter<MongoFrameworkQueryableModel>(connection);
			entityContainer.Update(new MongoFrameworkQueryableModel { Title = "EntityProcessorNoFireTest" }, EntityEntryState.Added);
			writer.Write(entityContainer);

			foreach (var titles in queryable.Select(e => e.Title))
			{
				//Do nothing
			}

			Assert.IsFalse(processor.EntityProcessed);
		}
	}
}
