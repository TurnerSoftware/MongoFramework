using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;
using MongoFramework.Tests.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MongoFramework.Tests
{
	[TestClass]
	public class DbEntityWriterTests
	{
		[TestMethod]
		public void AddEntity()
		{
			var database = TestConfiguration.GetDatabase();
			var entityContainer = new DbEntityCollection<EntityWriterModel>();
			var writer = new DbEntityWriter<EntityWriterModel>(database);

			var entity = new EntityWriterModel
			{
				Title = "DbEntityWriterTests.AddEntity"
			};

			entityContainer.Update(entity, DbEntityEntryState.Added);
			writer.Write(entityContainer);

			Assert.IsNotNull(entity.Id);
		}

		[TestMethod]
		public void AddMixedTypeEntities()
		{
			var database = TestConfiguration.GetDatabase();
			var entityContainer = new DbEntityCollection<EntityWriterModel>();
			var writer = new DbEntityWriter<EntityWriterModel>(database);
			var entities = new[]
			{
				new EntityWriterModel
				{
					Title = "DbEntityWriterTests.AddMixedTypeEntities"
				},
				new ExtendedEntityWriterModel
				{
					Title = "DbEntityWriterTests.AddMixedTypeEntities",
					AdditionalField = "AdditionalFieldSet"
				}
			};

			foreach (var entity in entities)
			{
				entityContainer.Update(entity, DbEntityEntryState.Added);
			}

			writer.Write(entityContainer);

			Assert.IsTrue(entities.OfType<EntityWriterModel>().Any(e => e.Title == "DbEntityWriterTests.AddMixedTypeEntities"));
			Assert.IsTrue(entities.OfType<ExtendedEntityWriterModel>().Any(e => e.AdditionalField == "AdditionalFieldSet"));
		}

		[TestMethod]
		public void UpdateEntity()
		{
			var database = TestConfiguration.GetDatabase();
			var entityContainer = new DbEntityCollection<EntityWriterModel>();
			var writer = new DbEntityWriter<EntityWriterModel>(database);
			var reader = new DbEntityReader<EntityWriterModel>(database);

			//Get entity initially into the DB so we can update it
			var entity = new EntityWriterModel
			{
				Title = "DbEntityWriterTests.UpdateEntity"
			};

			entityContainer.Update(entity, DbEntityEntryState.Added);
			writer.Write(entityContainer);
			entityContainer.Clear();

			//Our updated entity with the same ID
			var updatedEntity = new EntityWriterModel
			{
				Id = entity.Id,
				Title = "DbEntityWriterTests.UpdateEntity-Updated"
			};

			entityContainer.Update(updatedEntity, DbEntityEntryState.Updated);
			writer.Write(entityContainer);

			var dbEntity = reader.AsQueryable().Where(e => e.Id == entity.Id).FirstOrDefault();
			Assert.AreEqual("DbEntityWriterTests.UpdateEntity-Updated", dbEntity.Title);
		}

		[TestMethod]
		public void RemoveEntity()
		{
			var database = TestConfiguration.GetDatabase();
			var entityContainer = new DbEntityCollection<EntityWriterModel>();
			var writer = new DbEntityWriter<EntityWriterModel>(database);
			var reader = new DbEntityReader<EntityWriterModel>(database);

			//Get entity initially into the DB so we can remove it
			var entity = new EntityWriterModel
			{
				Title = "DbEntityWriterTests.RemoveEntity"
			};

			entityContainer.Update(entity, DbEntityEntryState.Added);
			writer.Write(entityContainer);
			entityContainer.Clear();

			//Remove the entity
			entityContainer.Update(entity, DbEntityEntryState.Deleted);
			writer.Write(entityContainer);

			Assert.IsFalse(reader.AsQueryable().Any(e => e.Id == entity.Id));
		}

		[TestMethod]
		public void MixedActionWrite()
		{
			var database = TestConfiguration.GetDatabase();
			var entityContainer = new DbEntityCollection<EntityWriterModel>();
			var writer = new DbEntityWriter<EntityWriterModel>(database);
			var reader = new DbEntityReader<EntityWriterModel>(database);

			var updateEntity = new EntityWriterModel
			{
				Title = "DbEntityWriterTests.MixedActionWrite-UpdateEntity"
			};
			var deleteEntity = new EntityWriterModel
			{
				Title = "DbEntityWriterTests.MixedActionWrite-DeleteEntity"
			};
			entityContainer.Update(updateEntity, DbEntityEntryState.Added);
			entityContainer.Update(deleteEntity, DbEntityEntryState.Added);
			writer.Write(entityContainer);
			entityContainer.Clear();

			var addedEntity = new EntityWriterModel
			{
				Title = "DbEntityWriterTests.MixedActionWrite-AddEntity"
			};
			updateEntity.Title = "DbEntityWriterTests.MixedActionWrite-UpdateEntity-Updated";
			entityContainer.Update(addedEntity, DbEntityEntryState.Added);
			entityContainer.Update(updateEntity, DbEntityEntryState.Updated);
			entityContainer.Update(deleteEntity, DbEntityEntryState.Deleted);
			writer.Write(entityContainer);

			Assert.IsTrue(reader.AsQueryable().Where(e => e.Id == addedEntity.Id).Any());
			Assert.IsFalse(reader.AsQueryable().Where(e => e.Id == deleteEntity.Id).Any());

			var dbEntity = reader.AsQueryable().Where(e => e.Id == updateEntity.Id).FirstOrDefault();
			Assert.AreEqual("DbEntityWriterTests.MixedActionWrite-UpdateEntity-Updated", dbEntity.Title);
		}

		[TestMethod]
		public async Task MixedActionWriteAsync()
		{
			var database = TestConfiguration.GetDatabase();
			var entityContainer = new DbEntityCollection<EntityWriterModel>();
			var writer = new DbEntityWriter<EntityWriterModel>(database);
			var reader = new DbEntityReader<EntityWriterModel>(database);

			var updateEntity = new EntityWriterModel
			{
				Title = "DbEntityWriterTests.MixedActionWriteAsync-UpdateEntity"
			};
			var deleteEntity = new EntityWriterModel
			{
				Title = "DbEntityWriterTests.MixedActionWriteAsync-DeleteEntity"
			};
			entityContainer.Update(updateEntity, DbEntityEntryState.Added);
			entityContainer.Update(deleteEntity, DbEntityEntryState.Added);
			await writer.WriteAsync(entityContainer);
			entityContainer.Clear();

			var addedEntity = new EntityWriterModel
			{
				Title = "DbEntityWriterTests.MixedActionWriteAsync-AddEntity"
			};
			updateEntity.Title = "DbEntityWriterTests.MixedActionWriteAsync-UpdateEntity-Updated";
			entityContainer.Update(addedEntity, DbEntityEntryState.Added);
			entityContainer.Update(updateEntity, DbEntityEntryState.Updated);
			entityContainer.Update(deleteEntity, DbEntityEntryState.Deleted);
			await writer.WriteAsync(entityContainer);

			Assert.IsTrue(reader.AsQueryable().Where(e => e.Id == addedEntity.Id).Any());
			Assert.IsFalse(reader.AsQueryable().Where(e => e.Id == deleteEntity.Id).Any());

			var dbEntity = reader.AsQueryable().Where(e => e.Id == updateEntity.Id).FirstOrDefault();
			Assert.AreEqual("DbEntityWriterTests.MixedActionWriteAsync-UpdateEntity-Updated", dbEntity.Title);
		}
	}
}