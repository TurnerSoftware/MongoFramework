using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;
using MongoFramework.Tests.Models;

namespace MongoFramework.Tests
{
	[TestClass]
	public class AsyncDbEntityWriterTests
	{
		[TestMethod]
		public async Task AddEntitiesAsync()
		{
			var database = TestConfiguration.GetDatabase();
			var writer = new AsyncDbEntityWriter<CommonEntity>(database);
			var entities = new[]
			{
				new CommonEntity
				{
					Description = "AsyncDbEntityWriterTests.AddEntities"
				},
				new CommonEntity
				{
					Description = "AsyncDbEntityWriterTests.AddEntities"
				},
				new CommonEntity
				{
					Description = "AsyncDbEntityWriterTests.AddEntities"
				}
			};

			await writer.AddRangeAsync(entities);

			Assert.IsTrue(entities.All(e => e.Id != null));
		}

		[TestMethod]
		public async Task UpdateEntityAsync()
		{
			var database = TestConfiguration.GetDatabase();
			var writer = new AsyncDbEntityWriter<CommonEntity>(database);
			var reader = new DbEntityReader<CommonEntity>(database);

			//Get entity initially into the DB so we can update it
			var entity = new CommonEntity
			{
				Description = "AsyncDbEntityWriterTests.UpdateEntity"
			};
			await writer.AddAsync(entity);

			//Our updated entity with the same ID
			var updatedEntity = new CommonEntity
			{
				Id = entity.Id,
				Description = "AsyncDbEntityWriterTests.UpdateEntity-Updated"
			};
			await writer.UpdateAsync(updatedEntity);

			var dbEntity = reader.AsQueryable().Where(e => e.Id == entity.Id).FirstOrDefault();
			Assert.AreEqual("AsyncDbEntityWriterTests.UpdateEntity-Updated", dbEntity.Description);
		}

		[TestMethod]
		public async Task RemoveEntityAsync()
		{
			var database = TestConfiguration.GetDatabase();
			var writer = new AsyncDbEntityWriter<CommonEntity>(database);
			var reader = new DbEntityReader<CommonEntity>(database);

			//Get entity initially into the DB so we can remove it
			var entity = new CommonEntity
			{
				Description = "DbEntityWriterTests.RemoveEntity"
			};
			await writer.AddAsync(entity);

			//Remove the entity
			await writer.RemoveAsync(entity);

			var dbEntity = reader.AsQueryable().Where(e => e.Id == entity.Id).FirstOrDefault();
			Assert.IsNull(dbEntity);
		}
	}
}