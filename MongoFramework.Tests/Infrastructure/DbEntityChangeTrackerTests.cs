using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;
using System.Linq;

namespace MongoFramework.Tests.Infrastructure
{
	[TestClass]
	public class DbChangeTrackerTests : TestBase
	{
		public class ChangeTrackerModel
		{
			public string Id { get; set; }
			public string Title { get; set; }
		}

		[TestMethod]
		public void DetectChangesDoesntCountAddedEntries()
		{
			var changeTracker = new DbEntityChangeTracker<ChangeTrackerModel>();
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
			var changeTracker = new DbEntityChangeTracker<ChangeTrackerModel>();
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
			var changeTracker = new DbEntityChangeTracker<ChangeTrackerModel>();
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
		public void CommittedChangesAreUpdated()
		{
			var changeTracker = new DbEntityChangeTracker<ChangeTrackerModel>();

			var addedEntity = new ChangeTrackerModel();
			var updatedEntity = new ChangeTrackerModel();
			var deletedEntity = new ChangeTrackerModel();

			changeTracker.Update(addedEntity, DbEntityEntryState.Added);
			changeTracker.Update(updatedEntity, DbEntityEntryState.Updated);
			changeTracker.Update(deletedEntity, DbEntityEntryState.Deleted);

			changeTracker.CommitChanges();

			Assert.AreEqual(DbEntityEntryState.NoChanges, changeTracker.GetEntry(addedEntity).State);
			Assert.AreEqual(DbEntityEntryState.NoChanges, changeTracker.GetEntry(updatedEntity).State);
			Assert.AreEqual(2, changeTracker.GetEntries().Count());
		}
	}
}