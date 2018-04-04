using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.EntityRelationships;

namespace MongoFramework.Tests.EntityRelationships.EntityCollection
{
	[TestClass]
	public class EntityCollectionTests : TestBase
	{
		[TestMethod]
		public void AddItemsToNewEntity()
		{
			var database = TestConfiguration.GetDatabase();
			var dbSet = new MongoDbSet<BaseEntityModel>();
			dbSet.SetDatabase(database);

			var entity = dbSet.Create();
			entity.Description = "AddItemsToNewEntity";

			entity.RelatedEntities.Add(new RelatedEntityModel
			{
				Description = "AddItemsToNewEntity-RelatedEntityModel-1"
			});
			entity.RelatedEntities.Add(new RelatedEntityModel
			{
				Description = "AddItemsToNewEntity-RelatedEntityModel-2"
			});

			dbSet.SaveChanges();

			var dbEntity = dbSet.Where(e => e.Id == entity.Id).FirstOrDefault();

			Assert.AreEqual(2, dbEntity.RelatedEntities.Count);
			Assert.IsTrue(dbEntity.RelatedEntities.All(e => e.Id != null));
		}

		[TestMethod]
		public void AddItemsToExistingEntity()
		{
			var database = TestConfiguration.GetDatabase();
			var dbSet = new MongoDbSet<BaseEntityModel>();
			dbSet.SetDatabase(database);

			var entity = dbSet.Create();
			entity.Description = "AddItemsToExistingEntity";

			dbSet.SaveChanges();

			var dbEntity = dbSet.Where(e => e.Id == entity.Id).FirstOrDefault();

			dbEntity.RelatedEntities.Add(new RelatedEntityModel
			{
				Description = "AddItemsToExistingEntity-RelatedEntityModel-1"
			});
			dbEntity.RelatedEntities.Add(new RelatedEntityModel
			{
				Description = "AddItemsToExistingEntity-RelatedEntityModel-2"
			});

			dbSet.SaveChanges();

			Assert.AreEqual(2, dbEntity.RelatedEntities.Count);
			Assert.IsTrue(dbEntity.RelatedEntities.All(e => e.Id != null));
		}

		[TestMethod]
		public void UpdateCollectionItem()
		{
			var database = TestConfiguration.GetDatabase();
			var dbSet = new MongoDbSet<BaseEntityModel>();
			dbSet.SetDatabase(database);

			var entity = dbSet.Create();
			entity.Description = "UpdateCollectionItem";

			var item = new RelatedEntityModel
			{
				Description = "UpdateCollectionItem-RelatedEntityModel-1"
			};
			entity.RelatedEntities.Add(item);

			dbSet.SaveChanges();

			item.Description = "UpdateCollectionItem-RelatedEntityModel-1-Updated";

			dbSet.SaveChanges();

			var dbEntity = dbSet.Where(e => e.Id == entity.Id).FirstOrDefault();

			Assert.AreEqual("UpdateCollectionItem-RelatedEntityModel-1-Updated", dbEntity.RelatedEntities.FirstOrDefault().Description);
		}

		[TestMethod]
		public void RemoveCollectionItem()
		{
			var database = TestConfiguration.GetDatabase();
			var dbSet = new MongoDbSet<BaseEntityModel>();
			dbSet.SetDatabase(database);

			var entity = dbSet.Create();
			entity.Description = "RemoveCollectionItem";

			var item = new RelatedEntityModel
			{
				Description = "RemoveCollectionItem-RelatedEntityModel-1"
			};
			entity.RelatedEntities.Add(item);

			dbSet.SaveChanges();

			entity.RelatedEntities.Remove(item);

			dbSet.SaveChanges();

			var dbEntity = dbSet.Where(e => e.Id == entity.Id).FirstOrDefault();
			Assert.AreEqual(0, dbEntity.RelatedEntities.Count);

			var collectionDbSet = new MongoDbSet<RelatedEntityModel>();
			collectionDbSet.SetDatabase(database);
			var itemDbEntity = collectionDbSet.Where(e => e.Id == item.Id).FirstOrDefault();

			Assert.IsNotNull(itemDbEntity);
		}

		[TestMethod]
		public void SaveWithNullNavigationProperty()
		{
			var database = TestConfiguration.GetDatabase();
			var dbSet = new MongoDbSet<BaseEntityModel>();
			dbSet.SetDatabase(database);

			var entity = new BaseEntityModel
			{
				Description = "SaveWithNullNavigationProperty"
			};

			dbSet.Add(entity);
			dbSet.SaveChanges();

			var dbEntity = dbSet.Where(e => e.Id == entity.Id).FirstOrDefault();

			Assert.AreEqual(0, dbEntity.RelatedEntities.Count);
		}
	}
}
