using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.EntityRelationships;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mutation.Mutators;
using MongoFramework.Infrastructure.Mutation;
using System;
using System.Linq;

namespace MongoFramework.Tests.EntityRelationships.SingleEntity
{
	[TestClass]
	public class SingleEntityTests : DbTest
	{
		[TestMethod]
		public void ForeignKeyAttributeOnId()
		{
			var entityMapper = new EntityMapper<BaseEntityModel>();
			var relationships = EntityMapperExtensions.GetEntityRelationships(entityMapper);

			var createdByIdProperty = typeof(BaseEntityModel).GetProperty("CreatedById");
			var attributeOnIdRelationship = relationships.Where(r => r.IdProperty == createdByIdProperty).FirstOrDefault();

			Assert.IsFalse(attributeOnIdRelationship.IsCollection);
			Assert.AreEqual(typeof(UserEntityModel), attributeOnIdRelationship.EntityType);
			Assert.AreEqual(typeof(BaseEntityModel).GetProperty("CreatedBy"), attributeOnIdRelationship.NavigationProperty);
		}

		[TestMethod]
		public void ForeignKeyAttributeOnNavigationProperty()
		{
			var entityMapper = new EntityMapper<BaseEntityModel>();
			var relationships = EntityMapperExtensions.GetEntityRelationships(entityMapper);

			var updatedByIdProperty = typeof(BaseEntityModel).GetProperty("UpdatedById");
			var attributeOnIdRelationship = relationships.Where(r => r.IdProperty == updatedByIdProperty).FirstOrDefault();

			Assert.IsFalse(attributeOnIdRelationship.IsCollection);
			Assert.AreEqual(typeof(UserEntityModel), attributeOnIdRelationship.EntityType);
			Assert.AreEqual(typeof(BaseEntityModel).GetProperty("UpdatedBy"), attributeOnIdRelationship.NavigationProperty);
		}

		[TestMethod]
		public void IdentifyRelationshipsWithOtherIdTypes()
		{
			var entityMapper = new EntityMapper<BaseVariedIdModel>();
			var relationships = EntityMapperExtensions.GetEntityRelationships(entityMapper);
			Assert.AreEqual(2, relationships.Count());
		}

		[TestMethod]
		[ExpectedException(typeof(MongoFrameworkMappingException))]
		public void UnsupportedIdTypeOnRelationship()
		{
			var entityMapper = new EntityMapper<UnsupportedIdModel>();
			var relationships = EntityMapperExtensions.GetEntityRelationships(entityMapper).ToArray();
		}

		[TestMethod]
		[ExpectedException(typeof(MongoFrameworkMappingException))]
		public void InvalidForeignKeyOnRelationship()
		{
			var entityMapper = new EntityMapper<InvalidForeignKeyModel>();
			var relationships = EntityMapperExtensions.GetEntityRelationships(entityMapper).ToArray();
		}

		[TestMethod]
		public void NavigationPropertiesUnmap()
		{
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

			var entityMapper = new EntityMapper<BaseEntityModel>();
			var entityRelationshipWriter = new EntityRelationshipWriter<BaseEntityModel>(database, entityMapper);

			entityRelationshipWriter.CommitEntityRelationships(new[] { baseEntity });

			Assert.IsNotNull(baseEntity.CreatedById);
			Assert.IsTrue(baseEntity.CreatedById == baseEntity.CreatedBy.Id);
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

			var entityMapper = new EntityMapper<BaseEntityModel>();
			new NavigationPropertyMutator<BaseEntityModel>().MutateEntity(baseEntity, MutatorType.Select, entityMapper, database);

			Assert.AreEqual("LoadEntity-UpdatedBy", baseEntity.UpdatedBy.Username);
		}
	}
}
