using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.EntityRelationships;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Tests.EntityRelationships.SingleEntity
{
	[TestClass]
	public class SingleEntityTests : DbTest
	{
		[TestMethod]
		public void ForeignKeyAttributeOnId()
		{
			var relationships = EntityRelationshipHelper.GetRelationshipsForType(typeof(BaseEntityModel));

			var createdByIdProperty = typeof(BaseEntityModel).GetProperty("CreatedById");
			var attributeOnIdRelationship = relationships.Where(r => r.IdProperty == createdByIdProperty).FirstOrDefault();

			Assert.IsFalse(attributeOnIdRelationship.IsCollection);
			Assert.AreEqual(typeof(UserEntityModel), attributeOnIdRelationship.EntityType);
			Assert.AreEqual(typeof(BaseEntityModel).GetProperty("CreatedBy"), attributeOnIdRelationship.NavigationProperty);
		}

		[TestMethod]
		public void ForeignKeyAttributeOnNavigationProperty()
		{
			var relationships = EntityRelationshipHelper.GetRelationshipsForType(typeof(BaseEntityModel));

			var updatedByIdProperty = typeof(BaseEntityModel).GetProperty("UpdatedById");
			var attributeOnIdRelationship = relationships.Where(r => r.IdProperty == updatedByIdProperty).FirstOrDefault();

			Assert.IsFalse(attributeOnIdRelationship.IsCollection);
			Assert.AreEqual(typeof(UserEntityModel), attributeOnIdRelationship.EntityType);
			Assert.AreEqual(typeof(BaseEntityModel).GetProperty("UpdatedBy"), attributeOnIdRelationship.NavigationProperty);
		}

		[TestMethod]
		public void IdentifyRelationshipsWithOtherIdTypes()
		{
			var relationships = EntityRelationshipHelper.GetRelationshipsForType(typeof(BaseVariedIdModel));
			Assert.AreEqual(2, relationships.Count());
		}

		[TestMethod]
		public void NavigationPropertiesUnmap()
		{
			var relationships = EntityRelationshipHelper.GetRelationshipsForType(typeof(BaseEntityModel));
			var entityMapper = new EntityMapper<BaseEntityModel>();

			Assert.IsFalse(entityMapper.GetEntityMapping().Any(e => e.FullPath == "CreatedBy" || e.FullPath == "UpdatedBy"));
		}

		[TestMethod]
		public void SaveNewEntity()
		{
			var database = TestConfiguration.GetDatabase();
			var baseEntity = new BaseEntityModel
			{
				CreatedBy = new UserEntityModel
				{
					Username = "SaveNewEntity-CreatedBy"
				}
			};

			var createdByRelationship = EntityRelationshipHelper.GetRelationshipsForType(typeof(BaseEntityModel)).Where(r => r.IdProperty.Name == "CreatedById").FirstOrDefault();
			EntityRelationshipHelper.SaveNavigationProperty(baseEntity, createdByRelationship, database);

			Assert.IsNotNull(baseEntity.CreatedById);
			Assert.IsTrue(baseEntity.CreatedById == baseEntity.CreatedBy.Id);
		}

		[TestMethod]
		public void SaveUpdatedEntity()
		{
			var database = TestConfiguration.GetDatabase();
			var baseEntity = new BaseEntityModel
			{
				CreatedBy = new UserEntityModel
				{
					Username = "SaveUpdatedEntity-CreatedBy"
				}
			};

			var createdByRelationship = EntityRelationshipHelper.GetRelationshipsForType(typeof(BaseEntityModel)).Where(r => r.IdProperty.Name == "CreatedById").FirstOrDefault();
			EntityRelationshipHelper.SaveNavigationProperty(baseEntity, createdByRelationship, database);

			baseEntity.CreatedBy.Username = "SaveUpdatedEntity-CreatedBy-Updated";

			EntityRelationshipHelper.SaveNavigationProperty(baseEntity, createdByRelationship, database);

			var dbEntityReader = new DbEntityReader<UserEntityModel>(database);
			var dbEntity = dbEntityReader.AsQueryable().Where(e => e.Id == baseEntity.CreatedBy.Id).FirstOrDefault();

			Assert.AreEqual("SaveUpdatedEntity-CreatedBy-Updated", dbEntity.Username);
		}

		[TestMethod]
		public void LoadEntity()
		{
			var database = TestConfiguration.GetDatabase();

			var userEntity = new UserEntityModel
			{
				Username = "LoadEntity-UpdatedBy"
			};
			var dbEntityWriter = new DbEntityWriter<UserEntityModel>(database);
			var collection = new DbEntityCollection<UserEntityModel>
			{
				userEntity
			};
			dbEntityWriter.Write(collection);
			
			var baseEntity = new BaseEntityModel
			{
				UpdatedById = userEntity.Id
			};

			var updatedByRelationship = EntityRelationshipHelper.GetRelationshipsForType(typeof(BaseEntityModel)).Where(r => r.IdProperty.Name == "UpdatedById").FirstOrDefault();
			EntityRelationshipHelper.LoadNavigationProperty(baseEntity, updatedByRelationship, database);

			Assert.AreEqual("LoadEntity-UpdatedBy", baseEntity.UpdatedBy.Username);
		}
	}
}
