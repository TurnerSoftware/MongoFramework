using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace MongoFramework.Tests.Infrastructure
{
	[TestClass]
	public class EntityWriterTests : TestBase
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

		[TestMethod]
		public void AddEntity()
		{
			var connection = TestConfiguration.GetConnection();
			var entityContainer = new EntityCollection<EntityWriterModel>();
			var writer = new EntityWriter<EntityWriterModel>(connection);

			var entity = new EntityWriterModel
			{
				Title = "DbEntityWriterTests.AddEntity"
			};

			entityContainer.Update(entity, EntityEntryState.Added);
			writer.Write(entityContainer);

			Assert.IsNotNull(entity.Id);
		}

		[TestMethod]
		public void AddMixedTypeEntities()
		{
			var connection = TestConfiguration.GetConnection();
			var entityContainer = new EntityCollection<EntityWriterModel>();
			var writer = new EntityWriter<EntityWriterModel>(connection);
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
				entityContainer.Update(entity, EntityEntryState.Added);
			}

			writer.Write(entityContainer);

			Assert.IsTrue(entities.OfType<EntityWriterModel>().Any(e => e.Title == "DbEntityWriterTests.AddMixedTypeEntities"));
			Assert.IsTrue(entities.OfType<ExtendedEntityWriterModel>().Any(e => e.AdditionalField == "AdditionalFieldSet"));
		}

		[TestMethod]
		public void UpdateEntity()
		{
			var connection = TestConfiguration.GetConnection();
			var entityContainer = new EntityCollection<EntityWriterModel>();
			var writer = new EntityWriter<EntityWriterModel>(connection);
			var reader = new EntityReader<EntityWriterModel>(connection);

			//Get entity initially into the DB so we can update it
			var entity = new EntityWriterModel
			{
				Title = "DbEntityWriterTests.UpdateEntity"
			};

			entityContainer.Update(entity, EntityEntryState.Added);
			writer.Write(entityContainer);
			entityContainer.Clear();

			//Our updated entity with the same ID
			var updatedEntity = new EntityWriterModel
			{
				Id = entity.Id,
				Title = "DbEntityWriterTests.UpdateEntity-Updated"
			};

			entityContainer.Update(updatedEntity, EntityEntryState.Updated);
			writer.Write(entityContainer);

			var dbEntity = reader.AsQueryable().Where(e => e.Id == entity.Id).FirstOrDefault();
			Assert.AreEqual("DbEntityWriterTests.UpdateEntity-Updated", dbEntity.Title);
		}

		[TestMethod]
		public void RemoveEntity()
		{
			var connection = TestConfiguration.GetConnection();
			var entityContainer = new EntityCollection<EntityWriterModel>();
			var writer = new EntityWriter<EntityWriterModel>(connection);
			var reader = new EntityReader<EntityWriterModel>(connection);

			//Get entity initially into the DB so we can remove it
			var entity = new EntityWriterModel
			{
				Title = "DbEntityWriterTests.RemoveEntity"
			};

			entityContainer.Update(entity, EntityEntryState.Added);
			writer.Write(entityContainer);
			entityContainer.Clear();

			//Remove the entity
			entityContainer.Update(entity, EntityEntryState.Deleted);
			writer.Write(entityContainer);

			Assert.IsFalse(reader.AsQueryable().Any(e => e.Id == entity.Id));
		}

		[TestMethod]
		public void MixedActionWrite()
		{
			var connection = TestConfiguration.GetConnection();
			var entityContainer = new EntityCollection<EntityWriterModel>();
			var writer = new EntityWriter<EntityWriterModel>(connection);
			var reader = new EntityReader<EntityWriterModel>(connection);

			var updateEntity = new EntityWriterModel
			{
				Title = "DbEntityWriterTests.MixedActionWrite-UpdateEntity"
			};
			var deleteEntity = new EntityWriterModel
			{
				Title = "DbEntityWriterTests.MixedActionWrite-DeleteEntity"
			};
			entityContainer.Update(updateEntity, EntityEntryState.Added);
			entityContainer.Update(deleteEntity, EntityEntryState.Added);
			writer.Write(entityContainer);
			entityContainer.Clear();

			var addedEntity = new EntityWriterModel
			{
				Title = "DbEntityWriterTests.MixedActionWrite-AddEntity"
			};
			updateEntity.Title = "DbEntityWriterTests.MixedActionWrite-UpdateEntity-Updated";
			entityContainer.Update(addedEntity, EntityEntryState.Added);
			entityContainer.Update(updateEntity, EntityEntryState.Updated);
			entityContainer.Update(deleteEntity, EntityEntryState.Deleted);
			writer.Write(entityContainer);

			Assert.IsTrue(reader.AsQueryable().Where(e => e.Id == addedEntity.Id).Any());
			Assert.IsFalse(reader.AsQueryable().Where(e => e.Id == deleteEntity.Id).Any());

			var dbEntity = reader.AsQueryable().Where(e => e.Id == updateEntity.Id).FirstOrDefault();
			Assert.AreEqual("DbEntityWriterTests.MixedActionWrite-UpdateEntity-Updated", dbEntity.Title);
		}

		[TestMethod]
		public async Task MixedActionWriteAsync()
		{
			var connection = TestConfiguration.GetConnection();
			var entityContainer = new EntityCollection<EntityWriterModel>();
			var writer = new EntityWriter<EntityWriterModel>(connection);
			var reader = new EntityReader<EntityWriterModel>(connection);

			var updateEntity = new EntityWriterModel
			{
				Title = "DbEntityWriterTests.MixedActionWriteAsync-UpdateEntity"
			};
			var deleteEntity = new EntityWriterModel
			{
				Title = "DbEntityWriterTests.MixedActionWriteAsync-DeleteEntity"
			};
			entityContainer.Update(updateEntity, EntityEntryState.Added);
			entityContainer.Update(deleteEntity, EntityEntryState.Added);
			await writer.WriteAsync(entityContainer).ConfigureAwait(false);
			entityContainer.Clear();

			var addedEntity = new EntityWriterModel
			{
				Title = "DbEntityWriterTests.MixedActionWriteAsync-AddEntity"
			};
			updateEntity.Title = "DbEntityWriterTests.MixedActionWriteAsync-UpdateEntity-Updated";
			entityContainer.Update(addedEntity, EntityEntryState.Added);
			entityContainer.Update(updateEntity, EntityEntryState.Updated);
			entityContainer.Update(deleteEntity, EntityEntryState.Deleted);
			await writer.WriteAsync(entityContainer).ConfigureAwait(false);

			Assert.IsTrue(reader.AsQueryable().Where(e => e.Id == addedEntity.Id).Any());
			Assert.IsFalse(reader.AsQueryable().Where(e => e.Id == deleteEntity.Id).Any());

			var dbEntity = reader.AsQueryable().Where(e => e.Id == updateEntity.Id).FirstOrDefault();
			Assert.AreEqual("DbEntityWriterTests.MixedActionWriteAsync-UpdateEntity-Updated", dbEntity.Title);
		}
	}
}