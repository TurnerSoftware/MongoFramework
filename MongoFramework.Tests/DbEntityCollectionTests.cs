using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;
using MongoFramework.Tests.Models;
using System;
using System.Linq;

namespace MongoFramework.Tests
{
	[TestClass]
	public class DbEntityCollectionTests : DbTest
	{
		[TestMethod]
		public void AddNewEntry()
		{
			var entityCollection = new DbEntityCollection<EntityContainerModel>();
			var entity = new EntityContainerModel
			{
				Title = "DbEntityCollectionTests.AddNewEntry"
			};
			entityCollection.Update(entity, DbEntityEntryState.Added);

			Assert.IsTrue(entityCollection.GetEntries().All(e => e.Entity == entity && e.State == DbEntityEntryState.Added));
		}

		[TestMethod]
		public void UpdateExistingEntryIdMatch()
		{
			var entityCollection = new DbEntityCollection<EntityContainerModel>();
			var entity = new EntityContainerModel
			{
				Id = "123",
				Title = "DbEntityCollectionTests.UpdateExistingEntryWithId-1"
			};
			entityCollection.Update(entity, DbEntityEntryState.Added);
			Assert.IsTrue(entityCollection.GetEntries().All(e => e.Entity == entity && e.State == DbEntityEntryState.Added));

			var updatedEntity = new EntityContainerModel
			{
				Id = "123",
				Title = "DbEntityCollectionTests.UpdateExistingEntryWithId-2"
			};
			entityCollection.Update(updatedEntity, DbEntityEntryState.Updated);
			Assert.IsFalse(entityCollection.GetEntries().Any(e => e.Entity == entity));
			Assert.IsTrue(entityCollection.GetEntries()
				.Any(e => e.Entity == updatedEntity && e.Entity.Title == "DbEntityCollectionTests.UpdateExistingEntryWithId-2"));
		}

		[TestMethod]
		public void UpdateExistingEntryInstanceMatch()
		{
			var entityCollection = new DbEntityCollection<EntityContainerModel>();
			var entity = new EntityContainerModel
			{
				Title = "DbEntityCollectionTests.UpdateExistingEntryWithoutId"
			};
			entityCollection.Update(entity, DbEntityEntryState.Added);
			Assert.IsTrue(entityCollection.GetEntries().All(e => e.Entity == entity && e.State == DbEntityEntryState.Added));

			entityCollection.Update(entity, DbEntityEntryState.NoChanges);
			Assert.IsTrue(entityCollection.GetEntries().All(e => e.Entity == entity && e.State == DbEntityEntryState.NoChanges));
		}

		[TestMethod]
		public void RemoveRange()
		{
			var entityCollection = new DbEntityCollection<EntityContainerModel>();
			var entities = new[]
			{
				new EntityContainerModel
				{
					Title = "DbEntityCollectionTests.RemoveRange-1"
				},
				new EntityContainerModel
				{
					Title = "DbEntityCollectionTests.RemoveRange-2"
				},
				new EntityContainerModel
				{
					Id = "123",
					Title = "DbEntityCollectionTests.RemoveRange-3"
				}
			};

			foreach (var entity in entities)
			{
				entityCollection.Update(entity, DbEntityEntryState.Added);
				entityCollection.Remove(entity);
			}

			Assert.IsFalse(entityCollection.GetEntries().Any());
		}

		[TestMethod]
		public void ClearTracker()
		{
			var entityCollection = new DbEntityCollection<EntityContainerModel>();
			var entity = new EntityContainerModel
			{
				Title = "DbEntityCollectionTests.ClearTracker"
			};
			entityCollection.Update(entity, DbEntityEntryState.NoChanges);

			entityCollection.Clear();

			Assert.IsFalse(entityCollection.GetEntries().Any());
		}

		[TestMethod]
		public void RemoveNonExistentEntities()
		{
			var entityCollection = new DbEntityCollection<EntityContainerModel>();
			Assert.IsFalse(entityCollection.Remove(new EntityContainerModel { }));
		}

		[TestMethod]
		public void ContainsExactEntity()
		{
			var entityCollection = new DbEntityCollection<EntityContainerModel>();
			var entity = new EntityContainerModel
			{
				Id = "ABC"
			};
			entityCollection.Add(entity);

			Assert.IsTrue(entityCollection.Contains(entity));
		}

		[TestMethod]
		public void ContainsEntityById()
		{
			var entityCollection = new DbEntityCollection<EntityContainerModel>();
			var entity = new EntityContainerModel
			{
				Id = "ABC",
				Title = "1"
			};
			entityCollection.Add(entity);

			var idMatchingEntity = new EntityContainerModel
			{
				Id = "ABC"
			};

			Assert.IsTrue(entityCollection.Contains(idMatchingEntity));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void CopyToInvalidArray()
		{
			var entityCollection = new DbEntityCollection<EntityContainerModel>();
			EntityContainerModel[] array = null;
			entityCollection.CopyTo(array, 0);
		}

		[TestMethod]
		[ExpectedException(typeof(IndexOutOfRangeException))]
		public void CopyToIndexOutOfRangeLow()
		{
			var entityCollection = new DbEntityCollection<EntityContainerModel>();
			var array = new EntityContainerModel[4];
			entityCollection.CopyTo(array, -1);
		}

		[TestMethod]
		[ExpectedException(typeof(IndexOutOfRangeException))]
		public void CopyToIndexOutOfRangeHigh()
		{
			var entityCollection = new DbEntityCollection<EntityContainerModel>
			{
				new EntityContainerModel { },
				new EntityContainerModel { },
				new EntityContainerModel { },
				new EntityContainerModel { }
			};

			var array = new EntityContainerModel[2];
			entityCollection.CopyTo(array, 1);
		}
	}
}