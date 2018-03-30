using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;
using MongoFramework.Tests.Models;
using System.Linq;

namespace MongoFramework.Tests
{
	[TestClass]
	public class DbEntityContainerTests
	{
		[TestMethod]
		public void AddNewEntry()
		{
			var entityContainer = new DbEntityContainer<EntityContainerModel>();
			var entity = new EntityContainerModel
			{
				Title = "DbEntityContainerTests.AddNewEntry"
			};
			entityContainer.Update(entity, DbEntityEntryState.Added);

			Assert.IsTrue(entityContainer.GetEntries().All(e => e.Entity == entity && e.State == DbEntityEntryState.Added));
		}

		[TestMethod]
		public void UpdateExistingEntryIdMatch()
		{
			var entityContainer = new DbEntityContainer<EntityContainerModel>();
			var entity = new EntityContainerModel
			{
				Id = "123",
				Title = "DbEntityContainerTests.UpdateExistingEntryWithId-1"
			};
			entityContainer.Update(entity, DbEntityEntryState.Added);
			Assert.IsTrue(entityContainer.GetEntries().All(e => e.Entity == entity && e.State == DbEntityEntryState.Added));

			var updatedEntity = new EntityContainerModel
			{
				Id = "123",
				Title = "DbEntityContainerTests.UpdateExistingEntryWithId-2"
			};
			entityContainer.Update(updatedEntity, DbEntityEntryState.Updated);
			Assert.IsFalse(entityContainer.GetEntries().Any(e => e.Entity == entity));
			Assert.IsTrue(entityContainer.GetEntries()
				.Any(e => e.Entity == updatedEntity && e.Entity.Title == "DbEntityContainerTests.UpdateExistingEntryWithId-2"));
		}

		[TestMethod]
		public void UpdateExistingEntryInstanceMatch()
		{
			var entityContainer = new DbEntityContainer<EntityContainerModel>();
			var entity = new EntityContainerModel
			{
				Title = "DbEntityContainerTests.UpdateExistingEntryWithoutId"
			};
			entityContainer.Update(entity, DbEntityEntryState.Added);
			Assert.IsTrue(entityContainer.GetEntries().All(e => e.Entity == entity && e.State == DbEntityEntryState.Added));

			entityContainer.Update(entity, DbEntityEntryState.NoChanges);
			Assert.IsTrue(entityContainer.GetEntries().All(e => e.Entity == entity && e.State == DbEntityEntryState.NoChanges));
		}

		[TestMethod]
		public void RemoveRange()
		{
			var entityContainer = new DbEntityContainer<EntityContainerModel>();
			var entities = new[]
			{
				new EntityContainerModel
				{
					Title = "DbEntityContainerTests.RemoveRange-1"
				},
				new EntityContainerModel
				{
					Title = "DbEntityContainerTests.RemoveRange-2"
				},
				new EntityContainerModel
				{
					Id = "123",
					Title = "DbEntityContainerTests.RemoveRange-3"
				}
			};

			foreach (var entity in entities)
			{
				entityContainer.Update(entity, DbEntityEntryState.Added);
				entityContainer.Remove(entity);
			}

			Assert.IsFalse(entityContainer.GetEntries().Any());
		}

		[TestMethod]
		public void ClearTracker()
		{
			var entityContainer = new DbEntityContainer<EntityContainerModel>();
			var entity = new EntityContainerModel
			{
				Title = "DbEntityContainerTests.ClearTracker"
			};
			entityContainer.Update(entity, DbEntityEntryState.NoChanges);

			entityContainer.Clear();

			Assert.IsFalse(entityContainer.GetEntries().Any());
		}
	}
}