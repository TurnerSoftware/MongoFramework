using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;

namespace MongoFramework.Tests
{
	public class EntityWriterModel
	{
		public string Id { get; set; }
		public string Title { get; set; }
	}

	public class ExtendedEntityWriterModel : EntityWriterModel
	{
		public string AdditionalField { get; set; }
	}

	[TestClass]
	public class DbEntityWriterTests
	{
		[TestMethod]
		public void AddEntity()
		{
			var database = TestConfiguration.GetDatabase();
			var writer = new DbEntityWriter<EntityWriterModel>(database);
			var entity = new EntityWriterModel
			{
				Title = "DbEntityWriterTests.AddEntity"
			};

			writer.Add(entity);

			Assert.IsNotNull(entity.Id);
		}

		[TestMethod]
		public void AddEntities()
		{
			var database = TestConfiguration.GetDatabase();
			var writer = new DbEntityWriter<EntityWriterModel>(database);
			var entities = new[]
			{
				new EntityWriterModel
				{
					Title = "DbEntityWriterTests.AddEntities"
				},
				new EntityWriterModel
				{
					Title = "DbEntityWriterTests.AddEntities"
				},
				new EntityWriterModel
				{
					Title = "DbEntityWriterTests.AddEntities"
				}
			};

			writer.AddRange(entities);

			Assert.IsTrue(entities.All(e => e.Id != null));
		}

		[TestMethod]
		public void AddMixedEntities()
		{
			var database = TestConfiguration.GetDatabase();
			var writer = new DbEntityWriter<EntityWriterModel>(database);
			var entities = new[]
			{
				new EntityWriterModel
				{
					Title = "DbEntityWriterTests.AddMixedEntities"
				},
				new ExtendedEntityWriterModel
				{
					Title = "DbEntityWriterTests.AddMixedEntities",
					AdditionalField = "AdditionalFieldSet"
				}
			};

			writer.AddRange(entities);

			Assert.IsTrue(entities.OfType<EntityWriterModel>().Any(e => e.Title == "DbEntityWriterTests.AddMixedEntities"));
			Assert.IsTrue(entities.OfType<ExtendedEntityWriterModel>().Any(e => e.AdditionalField == "AdditionalFieldSet"));
		}

		[TestMethod]
		public void UpdateEntity()
		{
			var database = TestConfiguration.GetDatabase();
			var writer = new DbEntityWriter<EntityWriterModel>(database);
			var reader = new DbEntityReader<EntityWriterModel>(database);

			//Get entity initially into the DB so we can update it
			var entity = new EntityWriterModel
			{
				Title = "DbEntityWriterTests.UpdateEntity"
			};
			writer.Add(entity);

			//Our updated entity with the same ID
			var updatedEntity = new EntityWriterModel
			{
				Id = entity.Id,
				Title = "DbEntityWriterTests.UpdateEntity-Updated"
			};
			writer.Update(updatedEntity);

			var dbEntity = reader.AsQueryable().Where(e => e.Id == entity.Id).FirstOrDefault();
			Assert.AreEqual("DbEntityWriterTests.UpdateEntity-Updated", dbEntity.Title);
		}

		[TestMethod]
		public void RemoveEntity()
		{
			var database = TestConfiguration.GetDatabase();
			var writer = new DbEntityWriter<EntityWriterModel>(database);
			var reader = new DbEntityReader<EntityWriterModel>(database);

			//Get entity initially into the DB so we can remove it
			var entity = new EntityWriterModel
			{
				Title = "DbEntityWriterTests.RemoveEntity"
			};
			writer.Add(entity);

			//Remove the entity
			writer.Remove(entity);

			Assert.IsFalse(reader.AsQueryable().Any(e => e.Id == entity.Id));
		}

		[TestMethod]
		public void AddViaChangeTracker()
		{
			var database = TestConfiguration.GetDatabase();
			var writer = new DbEntityWriter<EntityWriterModel>(database);
			var reader = new DbEntityReader<EntityWriterModel>(database);
			var changeTracker = new DbChangeTracker<EntityWriterModel>();

			var entity = new EntityWriterModel
			{
				Title = "DbEntityWriterTests.AddViaChangeTracker"
			};
			changeTracker.Update(entity, DbEntityEntryState.Added);

			writer.WriteChanges(changeTracker);

			Assert.IsTrue(reader.AsQueryable().Any(e => e.Id == entity.Id));
			Assert.AreEqual(DbEntityEntryState.NoChanges, changeTracker.GetEntry(entity).State);
		}

		[TestMethod]
		public void UpdatedViaChangeTracker()
		{
			var database = TestConfiguration.GetDatabase();
			var writer = new DbEntityWriter<EntityWriterModel>(database);
			var reader = new DbEntityReader<EntityWriterModel>(database);
			var changeTracker = new DbChangeTracker<EntityWriterModel>();

			var entity = new EntityWriterModel
			{
				Title = "DbEntityWriterTests.UpdatedViaChangeTracker"
			};
			changeTracker.Update(entity, DbEntityEntryState.Added);

			writer.WriteChanges(changeTracker);

			entity.Title = "DbEntityWriterTests.UpdatedViaChangeTracker-Updated";
			changeTracker.DetectChanges();

			Assert.AreEqual(DbEntityEntryState.Updated, changeTracker.GetEntry(entity).State);

			writer.WriteChanges(changeTracker);

			var dbEntity = reader.AsQueryable().Where(e => e.Id == entity.Id).FirstOrDefault();
			Assert.AreEqual("DbEntityWriterTests.UpdatedViaChangeTracker-Updated", dbEntity.Title);
			Assert.AreEqual(DbEntityEntryState.NoChanges, changeTracker.GetEntry(entity).State);
		}

		[TestMethod]
		public void UpdatedRangeViaChangeTracker()
		{
			var database = TestConfiguration.GetDatabase();
			var writer = new DbEntityWriter<EntityWriterModel>(database);
			var reader = new DbEntityReader<EntityWriterModel>(database);
			var changeTracker = new DbChangeTracker<EntityWriterModel>();

			var entities = new[]
			{
				new EntityWriterModel
				{
					Title = "DbEntityWriterTests.UpdatedRangeViaChangeTracker"
				},
				new EntityWriterModel
				{
					Title = "DbEntityWriterTests.UpdatedRangeViaChangeTracker"
				},
				new EntityWriterModel
				{
					Title = "DbEntityWriterTests.UpdatedRangeViaChangeTracker"
				}
			};
			changeTracker.UpdateRange(entities, DbEntityEntryState.Added);

			writer.WriteChanges(changeTracker);

			entities[0].Title = "DbEntityWriterTests.UpdatedRangeViaChangeTracker-Updated";
			changeTracker.DetectChanges();

			writer.WriteChanges(changeTracker);

			Assert.IsTrue(reader.AsQueryable().Any(e => e.Title == "DbEntityWriterTests.UpdatedRangeViaChangeTracker-Updated"));
		}

		[TestMethod]
		public void RemovedViaChangeTracker()
		{
			var database = TestConfiguration.GetDatabase();
			var writer = new DbEntityWriter<EntityWriterModel>(database);
			var reader = new DbEntityReader<EntityWriterModel>(database);
			var changeTracker = new DbChangeTracker<EntityWriterModel>();

			var entity = new EntityWriterModel
			{
				Title = "DbEntityWriterTests.UpdatedViaChangeTracker"
			};
			changeTracker.Update(entity, DbEntityEntryState.Added);

			writer.WriteChanges(changeTracker);

			changeTracker.Update(entity, DbEntityEntryState.Deleted);
			changeTracker.DetectChanges();

			writer.WriteChanges(changeTracker);

			Assert.IsFalse(reader.AsQueryable().Any(e => e.Id == entity.Id));
		}

		[TestMethod]
		public void RemovedRangeViaChangeTracker()
		{
			var database = TestConfiguration.GetDatabase();
			var writer = new DbEntityWriter<EntityWriterModel>(database);
			var reader = new DbEntityReader<EntityWriterModel>(database);
			var changeTracker = new DbChangeTracker<EntityWriterModel>();

			var entities = new[]
			{
				new EntityWriterModel
				{
					Title = "DbEntityWriterTests.RemovedRangeViaChangeTracker"
				},
				new EntityWriterModel
				{
					Title = "DbEntityWriterTests.RemovedRangeViaChangeTracker"
				},
				new EntityWriterModel
				{
					Title = "DbEntityWriterTests.RemovedRangeViaChangeTracker"
				}
			};
			changeTracker.UpdateRange(entities, DbEntityEntryState.Added);

			writer.WriteChanges(changeTracker);

			changeTracker.UpdateRange(entities, DbEntityEntryState.Deleted);
			changeTracker.DetectChanges();

			writer.WriteChanges(changeTracker);

			var addedEntityIds = entities.Select(e => e.Id);
			Assert.IsFalse(reader.AsQueryable().Any(e => addedEntityIds.Contains(e.Id)));
		}
	}
}