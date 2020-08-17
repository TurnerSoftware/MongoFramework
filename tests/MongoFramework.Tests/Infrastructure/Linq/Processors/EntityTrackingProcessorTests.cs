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
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var processor = new EntityTrackingProcessor<TestEntity>(context);

			processor.ProcessEntity(new TestEntity { Id = "123", Description = "Database" }, null);

			var collectionEntity = context.ChangeTracker.GetEntry(new TestEntity { Id = "123" }).Entity as TestEntity;
			Assert.AreEqual("Database", collectionEntity.Description);
		}

		[TestMethod]
		public void RefreshEntityIfMarkedAsNoChanges()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var processor = new EntityTrackingProcessor<TestEntity>(context);

			context.ChangeTracker.SetEntityState(new TestEntity
			{
				Id = "123",
				Description = "1"
			}, EntityEntryState.NoChanges);

			processor.ProcessEntity(new TestEntity { Id = "123", Description = "2" }, null);

			var collectionEntity = context.ChangeTracker.GetEntry(new TestEntity { Id = "123" }).Entity as TestEntity;
			Assert.AreEqual("2", collectionEntity.Description);
		}

		[TestMethod]
		public void DontRefreshEntityIfMarkedForDeletion()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var processor = new EntityTrackingProcessor<TestEntity>(context);

			context.ChangeTracker.SetEntityState(new TestEntity
			{
				Id = "123",
				Description = "Deleted"
			}, EntityEntryState.Deleted);

			processor.ProcessEntity(new TestEntity { Id = "123", Description = "Database" }, null);

			var collectionEntity = context.ChangeTracker.GetEntry(new TestEntity { Id = "123" }).Entity as TestEntity;
			Assert.AreEqual("Deleted", collectionEntity.Description);
		}

		[TestMethod]
		public void DontRefreshEntityIfMarkedForUpdate()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var processor = new EntityTrackingProcessor<TestEntity>(context);

			context.ChangeTracker.SetEntityState(new TestEntity
			{
				Id = "123",
				Description = "Updated"
			}, EntityEntryState.Updated);

			processor.ProcessEntity(new TestEntity { Id = "123", Description = "Database" }, null);

			var collectionEntity = context.ChangeTracker.GetEntry(new TestEntity { Id = "123" }).Entity as TestEntity;
			Assert.AreEqual("Updated", collectionEntity.Description);
		}

		[TestMethod]
		public void DontRefreshEntityIfMarkedForAdded()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var processor = new EntityTrackingProcessor<TestEntity>(context);

			context.ChangeTracker.SetEntityState(new TestEntity
			{
				Id = "123",
				Description = "Added"
			}, EntityEntryState.Added);

			processor.ProcessEntity(new TestEntity { Id = "123", Description = "Database" }, null);

			var collectionEntity = context.ChangeTracker.GetEntry(new TestEntity { Id = "123" }).Entity as TestEntity;
			Assert.AreEqual("Added", collectionEntity.Description);
		}
	}
}
