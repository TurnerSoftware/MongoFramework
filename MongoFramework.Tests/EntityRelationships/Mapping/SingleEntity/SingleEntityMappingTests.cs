using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure.EntityRelationships;
using MongoFramework.Infrastructure.Mapping;
using System.Linq;

namespace MongoFramework.Tests.EntityRelationships.Mapping.SingleEntity
{
	[TestClass]
	public class SingleEntityMappingTests : DbTest
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
	}
}
