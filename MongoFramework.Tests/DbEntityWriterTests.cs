using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;
using MongoFramework.Tests.Models;
using System.Linq;

namespace MongoFramework.Tests
{
	[TestClass]
	public class DbEntityWriterTests
	{
		[TestMethod]
		public void AddEntity()
		{
			var database = TestConfiguration.GetDatabase();
			var writer = new DbEntityWriter<CommonEntity>(database);
			var entity = new CommonEntity
			{
				Description = "DbEntityWriterTests.AddEntity"
			};

			writer.Add(entity);

			Assert.IsNotNull(entity.Id);
		}

		[TestMethod]
		public void AddEntities()
		{
			var database = TestConfiguration.GetDatabase();
			var writer = new DbEntityWriter<CommonEntity>(database);
			var entities = new[] {
				new CommonEntity
				{
					Description = "DbEntityWriterTests.AddEntities"
				},
				new CommonEntity
				{
					Description = "DbEntityWriterTests.AddEntities"
				},
				new CommonEntity
				{
					Description = "DbEntityWriterTests.AddEntities"
				}
			};

			writer.AddRange(entities);

			Assert.IsTrue(entities.All(e => e.Id != null));
		}

		[TestMethod]
		public void UpdateEntity()
		{
			var database = TestConfiguration.GetDatabase();
			var writer = new DbEntityWriter<CommonEntity>(database);
			var reader = new DbEntityReader<CommonEntity>(database);

			//Get entity initially into the DB so we can update it
			var entity = new CommonEntity
			{
				Description = "DbEntityWriterTests.UpdateEntity"
			};
			writer.Add(entity);

			//Our updated entity with the same ID
			var updatedEntity = new CommonEntity
			{
				Id = entity.Id,
				Description = "DbEntityWriterTests.UpdateEntity-Updated"
			};
			writer.Update(updatedEntity);

			var dbEntity = reader.AsQueryable().Where(e => e.Id == entity.Id).FirstOrDefault();
			Assert.AreEqual("DbEntityWriterTests.UpdateEntity-Updated", dbEntity.Description);
		}

		[TestMethod]
		public void DeleteEntity()
		{
			var database = TestConfiguration.GetDatabase();
			var writer = new DbEntityWriter<CommonEntity>(database);
			var reader = new DbEntityReader<CommonEntity>(database);

			//Get entity initially into the DB so we can delete it
			var entity = new CommonEntity
			{
				Description = "DbEntityWriterTests.DeleteEntity"
			};
			writer.Add(entity);

			//Remove the entity
			writer.Remove(entity);

			var dbEntity = reader.AsQueryable().Where(e => e.Id == entity.Id).FirstOrDefault();
			Assert.IsNull(dbEntity);
		}
	}
}
