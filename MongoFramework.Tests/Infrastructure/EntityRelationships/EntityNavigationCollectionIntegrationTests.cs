using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoFramework.Infrastructure.EntityRelationships;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MongoFramework.Tests.Infrastructure.EntityRelationships
{
	[TestClass]
	public class EntityNavigationCollectionIntegrationTests : TestBase
	{
		public class CollectionIntegrationModel
		{
			public string Id { get; set; }
			public string Description { get; set; }

			public virtual ICollection<StringIdModel> StringModelEntities { get; set; }
			public virtual ICollection<ObjectIdIdModel> ObjectIdModelEntities { get; set; }
			public virtual ICollection<GuidIdModel> GuidModelEntities { get; set; }

			[InverseProperty("SecondaryId")]
			public virtual ICollection<StringIdModel> InverseCollection { get; set; }
		}

		[TestMethod]
		public void AddRelationshipsToNewEntity()
		{
			var database = TestConfiguration.GetDatabase();
			var dbSet = new MongoDbSet<CollectionIntegrationModel>();
			dbSet.SetDatabase(database);

			var entity = dbSet.Create();
			entity.Description = "AddItemsToNewEntity";

			entity.StringModelEntities.Add(new StringIdModel
			{
				Description = "AddRelationshipsToNewEntity-StringIdModel-1"
			});
			entity.StringModelEntities.Add(new StringIdModel
			{
				Description = "AddRelationshipsToNewEntity-StringIdModel-2"
			});

			dbSet.SaveChanges();

			var dbEntity = dbSet.Where(e => e.Id == entity.Id).FirstOrDefault();

			Assert.AreEqual(2, dbEntity.StringModelEntities.Count);
			Assert.IsTrue(dbEntity.StringModelEntities.All(e => e.Id != null));
		}

		[TestMethod]
		public async Task AddRelationshipsToNewEntityAsync()
		{
			var database = TestConfiguration.GetDatabase();
			var dbSet = new MongoDbSet<CollectionIntegrationModel>();
			dbSet.SetDatabase(database);

			var entity = dbSet.Create();
			entity.Description = "AddRelationshipsToNewEntityAsync";

			entity.ObjectIdModelEntities.Add(new ObjectIdIdModel
			{
				Description = "AddRelationshipsToNewEntityAsync-ObjectIdIdModel-1"
			});
			entity.ObjectIdModelEntities.Add(new ObjectIdIdModel
			{
				Description = "AddRelationshipsToNewEntityAsync-ObjectIdIdModel-2"
			});

			await dbSet.SaveChangesAsync().ConfigureAwait(false);

			var dbEntity = dbSet.Where(e => e.Id == entity.Id).FirstOrDefault();

			Assert.AreEqual(2, dbEntity.ObjectIdModelEntities.Count);
			Assert.IsTrue(dbEntity.ObjectIdModelEntities.All(e => e.Id != ObjectId.Empty));
		}

		[TestMethod]
		public void AddRelationshipsToExistingEntity()
		{
			var database = TestConfiguration.GetDatabase();
			var dbSet = new MongoDbSet<CollectionIntegrationModel>();
			dbSet.SetDatabase(database);

			var entity = dbSet.Create();
			entity.Description = "AddRelationshipsToExistingEntity";

			dbSet.SaveChanges();

			var dbEntity = dbSet.Where(e => e.Id == entity.Id).FirstOrDefault();

			dbEntity.StringModelEntities.Add(new StringIdModel
			{
				Description = "AddRelationshipsToExistingEntity-StringIdModel-1"
			});
			dbEntity.StringModelEntities.Add(new StringIdModel
			{
				Description = "AddRelationshipsToExistingEntity-StringIdModel-2"
			});

			dbSet.SaveChanges();

			Assert.AreEqual(2, dbEntity.StringModelEntities.Count);
			Assert.IsTrue(dbEntity.StringModelEntities.All(e => e.Id != null));
		}

		[TestMethod]
		public void RemoveRelationshipToEntity()
		{
			var database = TestConfiguration.GetDatabase();
			var dbSet = new MongoDbSet<CollectionIntegrationModel>();
			dbSet.SetDatabase(database);

			var entity = dbSet.Create();
			entity.Description = "RemoveRelationshipToEntity";

			var item = new StringIdModel
			{
				Description = "RemoveRelationshipToEntity-StringIdModel-1"
			};
			entity.StringModelEntities.Add(item);

			dbSet.SaveChanges();

			entity.StringModelEntities.Remove(item);

			dbSet.SaveChanges();

			var dbEntity = dbSet.Where(e => e.Id == entity.Id).FirstOrDefault();
			Assert.AreEqual(0, dbEntity.StringModelEntities.Count);

			var collectionDbSet = new MongoDbSet<StringIdModel>();
			collectionDbSet.SetDatabase(database);
			var itemDbEntity = collectionDbSet.Where(e => e.Id == item.Id).FirstOrDefault();

			Assert.IsNotNull(itemDbEntity);
		}

		[TestMethod]
		public void SaveWithNullNavigationProperty()
		{
			var database = TestConfiguration.GetDatabase();
			var dbSet = new MongoDbSet<CollectionIntegrationModel>();
			dbSet.SetDatabase(database);

			var entity = new CollectionIntegrationModel
			{
				Description = "SaveWithNullNavigationProperty"
			};

			dbSet.Add(entity);
			dbSet.SaveChanges();

			var dbEntity = dbSet.Where(e => e.Id == entity.Id).FirstOrDefault();

			Assert.AreEqual(0, dbEntity.StringModelEntities.Count);
		}

		[TestMethod]
		public void ForceLoadEntities()
		{
			var database = TestConfiguration.GetDatabase();
			var dbSet = new MongoDbSet<CollectionIntegrationModel>();
			dbSet.SetDatabase(database);

			var entity = dbSet.Create();
			entity.Description = "ForceLoadEntities";

			entity.StringModelEntities.Add(new StringIdModel
			{
				Description = "ForceLoadEntities-StringIdModel-1"
			});
			entity.StringModelEntities.Add(new StringIdModel
			{
				Description = "ForceLoadEntities-StringIdModel-2"
			});

			dbSet.SaveChanges();

			var dbEntity = dbSet.Where(e => e.Id == entity.Id).FirstOrDefault();

			var navigationCollection = dbEntity.StringModelEntities as EntityNavigationCollection<StringIdModel>;

			Assert.AreEqual(2, navigationCollection.UnloadedCount);

			navigationCollection.LoadEntities();

			Assert.AreEqual(2, navigationCollection.LoadedCount);
			Assert.AreEqual(0, navigationCollection.UnloadedCount);
		}
	}
}
