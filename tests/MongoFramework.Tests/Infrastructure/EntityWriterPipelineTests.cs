using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace MongoFramework.Tests.Infrastructure
{
	[TestClass]
	public class EntityWriterPipelineTests : TestBase
	{
		public class TestModel
		{
			public string Id { get; set; }
			public string Title { get; set; }
		}

		public class ExtendedTestModel : TestModel
		{
			public string AdditionalField { get; set; }
		}

		[TestMethod]
		public void WriteFromCollection()
		{
			var connection = TestConfiguration.GetConnection();
			var entityCollection = new EntityCollection<TestModel>();
			var pipeline = new EntityWriterPipeline<TestModel>(connection);

			var entity = new TestModel
			{
				Title = "EntityWriterPipelineTests.WriteFromCollection"
			};
			entityCollection.Add(entity);

			pipeline.AddCollection(entityCollection);
			pipeline.Write();

			Assert.IsNotNull(entity.Id);
		}

		[TestMethod]
		public void WriteUpdatesCollection()
		{
			var connection = TestConfiguration.GetConnection();
			var entityCollection = new EntityCollection<TestModel>();
			var pipeline = new EntityWriterPipeline<TestModel>(connection);

			pipeline.AddCollection(entityCollection);

			var entity = new TestModel
			{
				Title = "EntityWriterPipelineTests.WriteUpdatesCollection"
			};
			entityCollection.Update(entity, EntityEntryState.Added);
			var entry = entityCollection.GetEntry(entity);

			Assert.AreEqual(EntityEntryState.Added, entry.State);
			pipeline.Write();
			Assert.AreEqual(EntityEntryState.NoChanges, entry.State);

			entry.State = EntityEntryState.Updated;
			pipeline.Write();
			Assert.AreEqual(EntityEntryState.NoChanges, entry.State);

			entry.State = EntityEntryState.Deleted;
			pipeline.Write();
			entry = entityCollection.GetEntry(entity);
			Assert.IsNull(entry);
		}

		[TestMethod]
		public void WriteFromStaging()
		{
			var connection = TestConfiguration.GetConnection();
			var pipeline = new EntityWriterPipeline<TestModel>(connection);

			var entity = new TestModel
			{
				Title = "EntityWriterPipelineTests.WriteFromCollection"
			};
			var command = EntityCommandBuilder<TestModel>.CreateCommand(
				new EntityEntry<TestModel>(entity, EntityEntryState.Added)
			);

			pipeline.StageCommand(command);
			pipeline.Write();

			Assert.IsNotNull(entity.Id);
		}

		[TestMethod]
		public void AddMixedTypeEntities()
		{
			var connection = TestConfiguration.GetConnection();
			var entityCollection = new EntityCollection<TestModel>();
			var pipeline = new EntityWriterPipeline<TestModel>(connection);
			var reader = new EntityReader<TestModel>(connection);

			pipeline.AddCollection(entityCollection);

			var entities = new[]
			{
				new TestModel
				{
					Title = "DbEntityWriterTests.AddMixedTypeEntities"
				},
				new ExtendedTestModel
				{
					Title = "DbEntityWriterTests.AddMixedTypeEntities",
					AdditionalField = "AdditionalFieldSet"
				}
			};

			foreach (var entity in entities)
			{
				entityCollection.Update(entity, EntityEntryState.Added);
			}

			pipeline.Write();

			Assert.IsTrue(reader.AsQueryable().OfType<TestModel>().Any(e => e.Title == "DbEntityWriterTests.AddMixedTypeEntities"));
			Assert.IsTrue(reader.AsQueryable().OfType<ExtendedTestModel>().Any(e => e.AdditionalField == "AdditionalFieldSet"));
		}

		[TestMethod]
		public void MixedActionWrite()
		{
			var connection = TestConfiguration.GetConnection();
			var entityCollection = new EntityCollection<TestModel>();
			var pipeline = new EntityWriterPipeline<TestModel>(connection);
			var reader = new EntityReader<TestModel>(connection);

			pipeline.AddCollection(entityCollection);

			var updateEntity = new TestModel
			{
				Title = "EntityWriterPipelineTests.MixedActionWrite-UpdateEntity"
			};
			var deleteEntity = new TestModel
			{
				Title = "EntityWriterPipelineTests.MixedActionWrite-DeleteEntity"
			};
			entityCollection.Update(updateEntity, EntityEntryState.Added);
			entityCollection.Update(deleteEntity, EntityEntryState.Added);
			pipeline.Write();
			entityCollection.Clear();

			var addedEntity = new TestModel
			{
				Title = "EntityWriterPipelineTests.MixedActionWrite-AddEntity"
			};
			updateEntity.Title = "EntityWriterPipelineTests.MixedActionWrite-UpdateEntity-Updated";
			entityCollection.Update(addedEntity, EntityEntryState.Added);
			entityCollection.Update(updateEntity, EntityEntryState.Updated);
			entityCollection.Update(deleteEntity, EntityEntryState.Deleted);
			pipeline.Write();

			Assert.IsTrue(reader.AsQueryable().Where(e => e.Id == addedEntity.Id).Any());
			Assert.IsFalse(reader.AsQueryable().Where(e => e.Id == deleteEntity.Id).Any());

			var dbEntity = reader.AsQueryable().Where(e => e.Id == updateEntity.Id).FirstOrDefault();
			Assert.AreEqual("EntityWriterPipelineTests.MixedActionWrite-UpdateEntity-Updated", dbEntity.Title);
		}

		[TestMethod]
		public async Task MixedActionWriteAsync()
		{
			var connection = TestConfiguration.GetConnection();
			var entityCollection = new EntityCollection<TestModel>();
			var pipeline = new EntityWriterPipeline<TestModel>(connection);
			var reader = new EntityReader<TestModel>(connection);

			pipeline.AddCollection(entityCollection);

			var updateEntity = new TestModel
			{
				Title = "EntityWriterPipelineTests.MixedActionWriteAsync-UpdateEntity"
			};
			var deleteEntity = new TestModel
			{
				Title = "EntityWriterPipelineTests.MixedActionWriteAsync-DeleteEntity"
			};
			entityCollection.Update(updateEntity, EntityEntryState.Added);
			entityCollection.Update(deleteEntity, EntityEntryState.Added);
			await pipeline.WriteAsync().ConfigureAwait(false);
			entityCollection.Clear();

			var addedEntity = new TestModel
			{
				Title = "EntityWriterPipelineTests.MixedActionWriteAsync-AddEntity"
			};
			updateEntity.Title = "EntityWriterPipelineTests.MixedActionWriteAsync-UpdateEntity-Updated";
			entityCollection.Update(addedEntity, EntityEntryState.Added);
			entityCollection.Update(updateEntity, EntityEntryState.Updated);
			entityCollection.Update(deleteEntity, EntityEntryState.Deleted);
			await pipeline.WriteAsync().ConfigureAwait(false);

			Assert.IsTrue(reader.AsQueryable().Where(e => e.Id == addedEntity.Id).Any());
			Assert.IsFalse(reader.AsQueryable().Where(e => e.Id == deleteEntity.Id).Any());

			var dbEntity = reader.AsQueryable().Where(e => e.Id == updateEntity.Id).FirstOrDefault();
			Assert.AreEqual("EntityWriterPipelineTests.MixedActionWriteAsync-UpdateEntity-Updated", dbEntity.Title);
		}
	}
}