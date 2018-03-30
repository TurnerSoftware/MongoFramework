using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;
using MongoFramework.Tests.Models;
using System.Linq;

namespace MongoFramework.Tests
{
	[TestClass]
	public class DbChangeTrackerTests
	{
		[TestMethod]
		public void AddNewEntry()
		{
			var changeTracker = new DbChangeTracker<ChangeTrackerModel>();
			var entity = new ChangeTrackerModel
			{
				Title = "DbChangeTrackerTests.AddNewEntry"
			};
			changeTracker.Update(entity, DbEntityEntryState.Added);

			Assert.IsTrue(changeTracker.GetEntries().All(e => e.Entity == entity && e.State == DbEntityEntryState.Added));
		}

		[TestMethod]
		public void AddNewEntries()
		{
			var changeTracker = new DbChangeTracker<ChangeTrackerModel>();
			var entities = new[]
			{
				new ChangeTrackerModel
				{
					Title = "DbChangeTrackerTests.AddNewEntries-1"
				},
				new ChangeTrackerModel
				{
					Title = "DbChangeTrackerTests.AddNewEntries-2"
				},
				new ChangeTrackerModel
				{
					Id = "123",
					Title = "DbChangeTrackerTests.AddNewEntries-3"
				}
			};
			changeTracker.UpdateRange(entities, DbEntityEntryState.Added);

			var trackedEntries = changeTracker.GetEntries();
			Assert.IsTrue(entities.All(e => trackedEntries.Any(te => te.Entity == e && te.State == DbEntityEntryState.Added)));
		}

		[TestMethod]
		public void UpdateExistingEntryIdMatch()
		{
			var changeTracker = new DbChangeTracker<ChangeTrackerModel>();
			var entity = new ChangeTrackerModel
			{
				Id = "123",
				Title = "UpdateExistingEntryWithId-1"
			};
			changeTracker.Update(entity, DbEntityEntryState.Added);
			Assert.IsTrue(changeTracker.GetEntries().All(e => e.Entity == entity && e.State == DbEntityEntryState.Added));

			var updatedEntity = new ChangeTrackerModel
			{
				Id = "123",
				Title = "UpdateExistingEntryWithId-2"
			};
			changeTracker.Update(updatedEntity, DbEntityEntryState.Updated);
			Assert.IsFalse(changeTracker.GetEntries().Any(e => e.Entity == entity));
			Assert.IsTrue(changeTracker.GetEntries()
				.Any(e => e.Entity == updatedEntity && e.Entity.Title == "UpdateExistingEntryWithId-2"));
		}

		[TestMethod]
		public void UpdateExistingEntryInstanceMatch()
		{
			var changeTracker = new DbChangeTracker<ChangeTrackerModel>();
			var entity = new ChangeTrackerModel
			{
				Title = "UpdateExistingEntryWithoutId"
			};
			changeTracker.Update(entity, DbEntityEntryState.Added);
			Assert.IsTrue(changeTracker.GetEntries().All(e => e.Entity == entity && e.State == DbEntityEntryState.Added));

			changeTracker.Update(entity, DbEntityEntryState.NoChanges);
			Assert.IsTrue(changeTracker.GetEntries().All(e => e.Entity == entity && e.State == DbEntityEntryState.NoChanges));
		}

		[TestMethod]
		public void DetectChangesDoesntCountAddedEntries()
		{
			var changeTracker = new DbChangeTracker<ChangeTrackerModel>();
			var entity = new ChangeTrackerModel
			{
				Title = "DbChangeTrackerTests.DetectChangesWhenNoneExist"
			};
			changeTracker.Update(entity, DbEntityEntryState.Added);

			changeTracker.DetectChanges();

			Assert.IsTrue(changeTracker.GetEntries().All(e => e.State == DbEntityEntryState.Added));
		}

		[TestMethod]
		public void DetectAnyChanges()
		{
			var changeTracker = new DbChangeTracker<ChangeTrackerModel>();
			var entity = new ChangeTrackerModel
			{
				Title = "DbChangeTrackerTests.DetectAnyChanges"
			};
			changeTracker.Update(entity, DbEntityEntryState.NoChanges);

			entity.Title = "DbChangeTrackerTests.DetectAnyChanges-Changed";

			changeTracker.DetectChanges();

			Assert.IsTrue(changeTracker.GetEntries().All(e => e.State == DbEntityEntryState.Updated));
		}

		[TestMethod]
		public void DetectAnyChangesThenChangedBackToOriginal()
		{
			var changeTracker = new DbChangeTracker<ChangeTrackerModel>();
			var entity = new ChangeTrackerModel
			{
				Title = "DbChangeTrackerTests.DetectAnyChangesThenChangedBackToOriginal"
			};
			changeTracker.Update(entity, DbEntityEntryState.NoChanges);

			entity.Title = "DbChangeTrackerTests.DetectAnyChangesThenChangedBackToOriginal-Changed";

			changeTracker.DetectChanges();

			entity.Title = "DbChangeTrackerTests.DetectAnyChangesThenChangedBackToOriginal";

			changeTracker.DetectChanges();

			Assert.IsTrue(changeTracker.GetEntries().All(e => e.State == DbEntityEntryState.NoChanges));
		}

		[TestMethod]
		public void RemoveRange()
		{
			var changeTracker = new DbChangeTracker<ChangeTrackerModel>();
			var entities = new[]
			{
				new ChangeTrackerModel
				{
					Title = "DbChangeTrackerTests.RemoveRange-1"
				},
				new ChangeTrackerModel
				{
					Title = "DbChangeTrackerTests.RemoveRange-2"
				},
				new ChangeTrackerModel
				{
					Id = "123",
					Title = "DbChangeTrackerTests.RemoveRange-3"
				}
			};
			changeTracker.UpdateRange(entities, DbEntityEntryState.Added);

			changeTracker.RemoveRange(entities);

			Assert.IsFalse(changeTracker.GetEntries().Any());
		}

		[TestMethod]
		public void ClearTracker()
		{
			var changeTracker = new DbChangeTracker<ChangeTrackerModel>();
			var entity = new ChangeTrackerModel
			{
				Title = "DbChangeTrackerTests.DetectChangesWhenSomeExist"
			};
			changeTracker.Update(entity, DbEntityEntryState.NoChanges);

			changeTracker.Clear();

			Assert.IsFalse(changeTracker.GetEntries().Any());
		}
	}
}