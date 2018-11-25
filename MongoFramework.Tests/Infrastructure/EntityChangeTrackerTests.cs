using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;
using System.Linq;

namespace MongoFramework.Tests.Infrastructure
{
	[TestClass]
	public class EntityChangeTrackerTests : TestBase
	{
		public class ChangeTrackerModel
		{
			public string Id { get; set; }
			public string Title { get; set; }
		}

		[TestMethod]
		public void DetectChangesDoesntCountAddedEntries()
		{
			var changeTracker = new EntityChangeTracker<ChangeTrackerModel>();
			var entity = new ChangeTrackerModel
			{
				Title = "DbChangeTrackerTests.DetectChangesWhenNoneExist"
			};
			changeTracker.Update(entity, EntityEntryState.Added);

			changeTracker.DetectChanges();

			Assert.IsTrue(changeTracker.GetEntries().All(e => e.State == EntityEntryState.Added));
		}

		[TestMethod]
		public void DetectAnyChanges()
		{
			var changeTracker = new EntityChangeTracker<ChangeTrackerModel>();
			var entity = new ChangeTrackerModel
			{
				Title = "DbChangeTrackerTests.DetectAnyChanges"
			};
			changeTracker.Update(entity, EntityEntryState.NoChanges);

			entity.Title = "DbChangeTrackerTests.DetectAnyChanges-Changed";

			changeTracker.DetectChanges();

			Assert.IsTrue(changeTracker.GetEntries().All(e => e.State == EntityEntryState.Updated));
		}

		[TestMethod]
		public void DetectAnyChangesThenChangedBackToOriginal()
		{
			var changeTracker = new EntityChangeTracker<ChangeTrackerModel>();
			var entity = new ChangeTrackerModel
			{
				Title = "DbChangeTrackerTests.DetectAnyChangesThenChangedBackToOriginal"
			};
			changeTracker.Update(entity, EntityEntryState.NoChanges);

			entity.Title = "DbChangeTrackerTests.DetectAnyChangesThenChangedBackToOriginal-Changed";

			changeTracker.DetectChanges();

			entity.Title = "DbChangeTrackerTests.DetectAnyChangesThenChangedBackToOriginal";

			changeTracker.DetectChanges();

			Assert.IsTrue(changeTracker.GetEntries().All(e => e.State == EntityEntryState.NoChanges));
		}

		[TestMethod]
		public void CommittedChangesAreUpdated()
		{
			var changeTracker = new EntityChangeTracker<ChangeTrackerModel>();

			var addedEntity = new ChangeTrackerModel();
			var updatedEntity = new ChangeTrackerModel();
			var deletedEntity = new ChangeTrackerModel();

			changeTracker.Update(addedEntity, EntityEntryState.Added);
			changeTracker.Update(updatedEntity, EntityEntryState.Updated);
			changeTracker.Update(deletedEntity, EntityEntryState.Deleted);

			changeTracker.CommitChanges();

			Assert.AreEqual(EntityEntryState.NoChanges, changeTracker.GetEntry(addedEntity).State);
			Assert.AreEqual(EntityEntryState.NoChanges, changeTracker.GetEntry(updatedEntity).State);
			Assert.AreEqual(2, changeTracker.GetEntries().Count());
		}
	}
}