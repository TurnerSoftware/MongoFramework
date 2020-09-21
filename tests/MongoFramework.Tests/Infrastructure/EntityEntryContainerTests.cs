using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;
using System;
using System.Linq;

namespace MongoFramework.Tests.Infrastructure
{
	[TestClass]
	public class EntityEntryContainerTests : TestBase
	{
		public class EntityEntryContainerModel
		{
			public string Id { get; set; }
			public string Title { get; set; }
		}

		public class EntityCollectionOverriddenEqualsModel
		{
			public string Id { get; set; }

			public string EqualsProperty { get; set; }

			public override bool Equals(object obj)
			{
				if (obj is EntityCollectionOverriddenEqualsModel model)
				{
					return EqualsProperty == model.EqualsProperty;
				}

				return false;
			}

			public override int GetHashCode()
			{
				return EqualsProperty?.GetHashCode() ?? base.GetHashCode();
			}
		}

		[TestMethod]
		public void AddNewEntry()
		{
			var entryContainer = new EntityEntryContainer();
			var entity = new EntityEntryContainerModel
			{
				Title = "EntityEntryContainerTests.AddNewEntry"
			};
			entryContainer.SetEntityState(entity, EntityEntryState.Added);

			var entry = entryContainer.GetEntry(entity);
			Assert.AreEqual(entity, entry.Entity);
			Assert.AreEqual(EntityEntryState.Added, entry.State);
		}

		[TestMethod]
		public void UpdateExistingEntryIdMatch()
		{
			var entryContainer = new EntityEntryContainer();
			var entity = new EntityEntryContainerModel
			{
				Id = "123",
				Title = "EntityEntryContainerTests.UpdateExistingEntryWithId-1"
			};
			entryContainer.SetEntityState(entity, EntityEntryState.Added);

			var entry = entryContainer.GetEntry(entity);
			Assert.AreEqual(entity, entry.Entity);
			Assert.AreEqual(EntityEntryState.Added, entry.State);

			var updatedEntity = new EntityEntryContainerModel
			{
				Id = "123",
				Title = "EntityEntryContainerTests.UpdateExistingEntryWithId-2"
			};
			entryContainer.SetEntityState(updatedEntity, EntityEntryState.Updated);

			Assert.IsFalse(entryContainer.Entries().Any(e => e.Entity == entity));

			entry = entryContainer.GetEntry(updatedEntity);
			Assert.AreEqual(updatedEntity, entry.Entity);
			Assert.AreEqual("EntityEntryContainerTests.UpdateExistingEntryWithId-2", (entry.Entity as EntityEntryContainerModel).Title);
		}

		[TestMethod]
		public void UpdateExistingEntryInstanceMatch()
		{
			var entryContainer = new EntityEntryContainer();
			var entity = new EntityEntryContainerModel
			{
				Title = "EntityEntryContainerTests.UpdateExistingEntryWithoutId"
			};
			entryContainer.SetEntityState(entity, EntityEntryState.Added);

			var entry = entryContainer.GetEntry(entity);
			Assert.AreEqual(entity, entry.Entity);
			Assert.AreEqual(EntityEntryState.Added, entry.State);

			entryContainer.SetEntityState(entity, EntityEntryState.NoChanges);

			entry = entryContainer.GetEntry(entity);
			Assert.AreEqual(entity, entry.Entity);
			Assert.AreEqual(EntityEntryState.NoChanges, entry.State);
		}

		[TestMethod]
		public void EntryDoesntMatchOnEqualityOverride()
		{
			var entryContainer = new EntityEntryContainer();
			var entityA = new EntityCollectionOverriddenEqualsModel
			{
				EqualsProperty = "EntityEntryContainerTests.EntityCollectionOverriddenEqualsModel"
			};
			entryContainer.SetEntityState(entityA, EntityEntryState.Added);

			var entityB = new EntityCollectionOverriddenEqualsModel
			{
				EqualsProperty = "EntityEntryContainerTests.EntityCollectionOverriddenEqualsModel"
			};
			entryContainer.SetEntityState(entityB, EntityEntryState.Added);
			Assert.AreEqual(2, entryContainer.Entries().Count());
		}

		[TestMethod]
		public void GetExistingEntryIdMatch()
		{
			var entryContainer = new EntityEntryContainer();
			var entity = new EntityEntryContainerModel
			{
				Id = "123",
				Title = "EntityEntryContainerTests.UpdateExistingEntryWithoutId"
			};
			entryContainer.SetEntityState(entity, EntityEntryState.Added);

			var entry = entryContainer.GetEntryById<EntityEntryContainerModel>("123");
			Assert.AreEqual(entity, entry.Entity);
			Assert.AreEqual(EntityEntryState.Added, entry.State);

			entryContainer.SetEntityState(entity, EntityEntryState.NoChanges);

			entry = entryContainer.GetEntryById<EntityEntryContainerModel>("123");
			Assert.AreEqual(entity, entry.Entity);
			Assert.AreEqual(EntityEntryState.NoChanges, entry.State);
		}

		[TestMethod]
		public void RemoveRange()
		{
			var entryContainer = new EntityEntryContainer();
			var entities = new[]
			{
				new EntityEntryContainerModel
				{
					Title = "EntityEntryContainerTests.RemoveRange-1"
				},
				new EntityEntryContainerModel
				{
					Title = "EntityEntryContainerTests.RemoveRange-2"
				},
				new EntityEntryContainerModel
				{
					Id = "123",
					Title = "EntityEntryContainerTests.RemoveRange-3"
				}
			};

			foreach (var entity in entities)
			{
				entryContainer.SetEntityState(entity, EntityEntryState.Added);
				entryContainer.SetEntityState(entity, EntityEntryState.Detached);
			}

			Assert.IsFalse(entryContainer.Entries().Any());
		}

		[TestMethod]
		public void ClearTracker()
		{
			var entryContainer = new EntityEntryContainer();
			var entity = new EntityEntryContainerModel
			{
				Title = "EntityEntryContainerTests.ClearTracker"
			};
			entryContainer.SetEntityState(entity, EntityEntryState.NoChanges);

			entryContainer.Clear();

			Assert.IsFalse(entryContainer.Entries().Any());
		}

		[TestMethod]
		public void RemoveNonExistentEntities()
		{
			var entryContainer = new EntityEntryContainer();
			Assert.IsNull(entryContainer.SetEntityState(new EntityEntryContainerModel { }, EntityEntryState.Detached));
		}
	}
}