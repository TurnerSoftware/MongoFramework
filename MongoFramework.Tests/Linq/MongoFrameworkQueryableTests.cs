using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Linq;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Tests.Models;
using System;
using System.Linq;

namespace MongoFramework.Tests.Linq
{
	[TestClass]
	public class MongoFrameworkQueryableTests
	{
		[TestMethod]
		public void EnumerateQueryable()
		{
			var database = TestConfiguration.GetDatabase();
			new EntityMapper<MongoFrameworkQueryableModel>();
			var collection = database.GetCollection<MongoFrameworkQueryableModel>("MongoFrameworkQueryableModel");
			var underlyingQueryable = collection.AsQueryable();
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel, MongoFrameworkQueryableModel>(underlyingQueryable);

			var entityContainer = new DbEntityCollection<MongoFrameworkQueryableModel>();
			var writer = new DbEntityWriter<MongoFrameworkQueryableModel>(database);
			entityContainer.Update(new MongoFrameworkQueryableModel { Title = "EnumerateQueryable" }, DbEntityEntryState.Added);
			writer.Write(entityContainer);

			foreach (var entity in queryable)
			{
				Assert.AreEqual("EnumerateQueryable", entity.Title);
			}
		}

		[TestMethod]
		public void EntityProcessorsFiresOnEnumerationOfTEntity()
		{
			var database = TestConfiguration.GetDatabase();
			new EntityMapper<MongoFrameworkQueryableModel>();
			var collection = database.GetCollection<MongoFrameworkQueryableModel>("MongoFrameworkQueryableModel");
			var underlyingQueryable = collection.AsQueryable();
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel, MongoFrameworkQueryableModel>(underlyingQueryable);

			var processor = new MongoFrameworkQueryableTestProcessor<MongoFrameworkQueryableModel>();
			queryable.EntityProcessors.Add(processor);

			var entityContainer = new DbEntityCollection<MongoFrameworkQueryableModel>();
			var writer = new DbEntityWriter<MongoFrameworkQueryableModel>(database);
			entityContainer.Update(new MongoFrameworkQueryableModel { Title = "EntityProcessorFireTest" }, DbEntityEntryState.Added);
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
			var database = TestConfiguration.GetDatabase();
			new EntityMapper<MongoFrameworkQueryableModel>();
			var collection = database.GetCollection<MongoFrameworkQueryableModel>("MongoFrameworkQueryableModel");
			var underlyingQueryable = collection.AsQueryable();
			var queryable = new MongoFrameworkQueryable<MongoFrameworkQueryableModel, MongoFrameworkQueryableModel>(underlyingQueryable);

			var processor = new MongoFrameworkQueryableTestProcessor<MongoFrameworkQueryableModel>();
			queryable.EntityProcessors.Add(processor);

			var entityContainer = new DbEntityCollection<MongoFrameworkQueryableModel>();
			var writer = new DbEntityWriter<MongoFrameworkQueryableModel>(database);
			entityContainer.Update(new MongoFrameworkQueryableModel { Title = "EntityProcessorNoFireTest" }, DbEntityEntryState.Added);
			writer.Write(entityContainer);

			foreach (var titles in queryable.Select(e => e.Title))
			{
				//Do nothing
			}

			Assert.IsFalse(processor.EntityProcessed);
		}
	}
}
