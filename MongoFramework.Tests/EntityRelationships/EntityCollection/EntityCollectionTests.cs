using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure.EntityRelationships;
using MongoFramework.Infrastructure.Mapping;
using System;
using System.Linq;

namespace MongoFramework.Tests.EntityRelationships.EntityCollection
{
	[TestClass]
	public class EntityCollectionTests : DbTest
	{
		[TestMethod]
		public void IdentifyCollectionRelationships()
		{
			var entityMapper = new EntityMapper<BaseEntityModel>();
			var relationships = EntityMapperExtensions.GetEntityRelationships(entityMapper);
			var relationship = relationships.FirstOrDefault();

			Assert.IsTrue(relationship.IsCollection);
			Assert.AreEqual(typeof(RelatedEntityModel), relationship.EntityType);
			Assert.AreEqual(typeof(RelatedEntityModel).GetProperty("Id"), relationship.IdProperty);
			Assert.AreEqual(typeof(BaseEntityModel).GetProperty("RelatedEntities"), relationship.NavigationProperty);
		}

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

		[TestMethod]
		public void InversePropertyMapping()
		{
			var entityMapper = new EntityMapper<InversePropertyModel>();
			var relationships = EntityMapperExtensions.GetEntityRelationships(entityMapper);

			Assert.IsTrue(relationships.Any(r => r.IsCollection && r.IdProperty.Name == "RelatedId"));
		}
	}
}
