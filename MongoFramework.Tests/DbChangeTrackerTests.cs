using System;
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
			var changeTracker = new DbChangeTracker<ExtendedEntity>();
			var entity = new ExtendedEntity
			{
				Description = "DbChangeTrackerTests.AddNewEntry"
			};
			changeTracker.Update(entity, DbEntityEntryState.Added);

			Assert.IsTrue(changeTracker.GetEntries().All(e => e.Entity == entity && e.State == DbEntityEntryState.Added));
		}

		[TestMethod]
		public void DetectChangesWhenNoneExist()
		{
			var changeTracker = new DbChangeTracker<ExtendedEntity>();
			var entity = new ExtendedEntity
			{
				Description = "DbChangeTrackerTests.DetectChangesWhenNoneExist"
			};
			changeTracker.Update(entity, DbEntityEntryState.Added);

			changeTracker.DetectChanges();

			Assert.IsFalse(changeTracker.GetEntries().All(e => e.State == DbEntityEntryState.NoChanges || e.State == DbEntityEntryState.Updated));
		}

		[TestMethod]
		public void DetectChangesWhenSomeExist()
		{
			var changeTracker = new DbChangeTracker<ExtendedEntity>();
			var entity = new ExtendedEntity
			{
				Description = "DbChangeTrackerTests.DetectChangesWhenSomeExist"
			};
			changeTracker.Update(entity, DbEntityEntryState.NoChanges);

			entity.Description = "DbChangeTrackerTests.DetectChangesWhenSomeExist-Changed";

			changeTracker.DetectChanges();

			Assert.IsTrue(changeTracker.GetEntries().All(e => e.State == DbEntityEntryState.Updated));
		}
	}
}
