using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Linq.Processors;

namespace MongoFramework.Tests.Infrastructure.Linq.Processors
{
	[TestClass]
	public class EntityTrackingProcessorTests : TestBase
	{
		public class TestEntity
		{
			public string Id { get; set; }
			public string Description { get; set; }
		}

		[TestMethod]
		public void TrackUnseenEntities()
		{
			var collection = new EntityCollection<TestEntity>();
			var processor = new EntityTrackingProcessor<TestEntity>(collection);

			processor.ProcessEntity(new TestEntity { Id = "123", Description = "Database" }, null);

			var collectionEntity = collection.GetEntry(new TestEntity { Id = "123" }).Entity as TestEntity;
			Assert.AreEqual("Database", collectionEntity.Description);
		}

		[TestMethod]
		public void RefreshEntityIfMarkedAsNoChanges()
		{
			var collection = new EntityCollection<TestEntity>();
			var processor = new EntityTrackingProcessor<TestEntity>(collection);

			collection.Update(new TestEntity
			{
				Id = "123",
				Description = "1"
			}, EntityEntryState.NoChanges);

			processor.ProcessEntity(new TestEntity { Id = "123", Description = "2" }, null);

			var collectionEntity = collection.GetEntry(new TestEntity { Id = "123" }).Entity as TestEntity;
			Assert.AreEqual("2", collectionEntity.Description);
		}

		[TestMethod]
		public void DontRefreshEntityIfMarkedForDeletion()
		{
			var collection = new EntityCollection<TestEntity>();
			var processor = new EntityTrackingProcessor<TestEntity>(collection);

			collection.Update(new TestEntity
			{
				Id = "123",
				Description = "Deleted"
			}, EntityEntryState.Deleted);

			processor.ProcessEntity(new TestEntity { Id = "123", Description = "Database" }, null);

			var collectionEntity = collection.GetEntry(new TestEntity { Id = "123" }).Entity as TestEntity;
			Assert.AreEqual("Deleted", collectionEntity.Description);
		}

		[TestMethod]
		public void DontRefreshEntityIfMarkedForUpdate()
		{
			var collection = new EntityCollection<TestEntity>();
			var processor = new EntityTrackingProcessor<TestEntity>(collection);

			collection.Update(new TestEntity
			{
				Id = "123",
				Description = "Updated"
			}, EntityEntryState.Updated);

			processor.ProcessEntity(new TestEntity { Id = "123", Description = "Database" }, null);

			var collectionEntity = collection.GetEntry(new TestEntity { Id = "123" }).Entity as TestEntity;
			Assert.AreEqual("Updated", collectionEntity.Description);
		}

		[TestMethod]
		public void DontRefreshEntityIfMarkedForAdded()
		{
			var collection = new EntityCollection<TestEntity>();
			var processor = new EntityTrackingProcessor<TestEntity>(collection);

			collection.Update(new TestEntity
			{
				Id = "123",
				Description = "Added"
			}, EntityEntryState.Added);

			processor.ProcessEntity(new TestEntity { Id = "123", Description = "Database" }, null);

			var collectionEntity = collection.GetEntry(new TestEntity { Id = "123" }).Entity as TestEntity;
			Assert.AreEqual("Added", collectionEntity.Description);
		}
	}
}
