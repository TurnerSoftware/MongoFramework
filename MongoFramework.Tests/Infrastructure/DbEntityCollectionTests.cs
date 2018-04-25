using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;
using System;
using System.Linq;

namespace MongoFramework.Tests
{
	[TestClass]
	public class DbEntityCollectionTests : TestBase
	{
		public class EntityCollectionModel
		{
			public string Id { get; set; }
			public string Title { get; set; }
		}

		[TestMethod]
		public void AddNewEntry()
		{
			var entityCollection = new DbEntityCollection<EntityCollectionModel>();
			var entity = new EntityCollectionModel
			{
				Title = "DbEntityCollectionTests.AddNewEntry"
			};
			entityCollection.Update(entity, DbEntityEntryState.Added);

			Assert.IsTrue(entityCollection.GetEntries().All(e => e.Entity == entity && e.State == DbEntityEntryState.Added));
		}

		[TestMethod]
		public void UpdateExistingEntryIdMatch()
		{
			var entityCollection = new DbEntityCollection<EntityCollectionModel>();
			var entity = new EntityCollectionModel
			{
				Id = "123",
				Title = "DbEntityCollectionTests.UpdateExistingEntryWithId-1"
			};
			entityCollection.Update(entity, DbEntityEntryState.Added);
			Assert.IsTrue(entityCollection.GetEntries().All(e => e.Entity == entity && e.State == DbEntityEntryState.Added));

			var updatedEntity = new EntityCollectionModel
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
			var entityCollection = new DbEntityCollection<EntityCollectionModel>();
			var entity = new EntityCollectionModel
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
			var entityCollection = new DbEntityCollection<EntityCollectionModel>();
			var entities = new[]
			{
				new EntityCollectionModel
				{
					Title = "DbEntityCollectionTests.RemoveRange-1"
				},
				new EntityCollectionModel
				{
					Title = "DbEntityCollectionTests.RemoveRange-2"
				},
				new EntityCollectionModel
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
			var entityCollection = new DbEntityCollection<EntityCollectionModel>();
			var entity = new EntityCollectionModel
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
			var entityCollection = new DbEntityCollection<EntityCollectionModel>();
			Assert.IsFalse(entityCollection.Remove(new EntityCollectionModel { }));
		}

		[TestMethod]
		public void ContainsExactEntity()
		{
			var entityCollection = new DbEntityCollection<EntityCollectionModel>();
			var entity = new EntityCollectionModel
			{
				Id = "ABC"
			};
			entityCollection.Add(entity);

			Assert.IsTrue(entityCollection.Contains(entity));
		}

		[TestMethod]
		public void ContainsEntityById()
		{
			var entityCollection = new DbEntityCollection<EntityCollectionModel>();
			var entity = new EntityCollectionModel
			{
				Id = "ABC",
				Title = "1"
			};
			entityCollection.Add(entity);

			var idMatchingEntity = new EntityCollectionModel
			{
				Id = "ABC"
			};

			Assert.IsTrue(entityCollection.Contains(idMatchingEntity));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void CopyToInvalidArray()
		{
			var entityCollection = new DbEntityCollection<EntityCollectionModel>();
			EntityCollectionModel[] array = null;
			entityCollection.CopyTo(array, 0);
		}

		[TestMethod]
		[ExpectedException(typeof(IndexOutOfRangeException))]
		public void CopyToIndexOutOfRangeLow()
		{
			var entityCollection = new DbEntityCollection<EntityCollectionModel>();
			var array = new EntityCollectionModel[4];
			entityCollection.CopyTo(array, -1);
		}

		[TestMethod]
		[ExpectedException(typeof(IndexOutOfRangeException))]
		public void CopyToIndexOutOfRangeHigh()
		{
			var entityCollection = new DbEntityCollection<EntityCollectionModel>
			{
				new EntityCollectionModel { },
				new EntityCollectionModel { },
				new EntityCollectionModel { },
				new EntityCollectionModel { }
			};

			var array = new EntityCollectionModel[2];
			entityCollection.CopyTo(array, 1);
		}
	}
}