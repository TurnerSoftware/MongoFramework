using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure.EntityRelationships;
using MongoFramework.Infrastructure.Mapping;
using System;
using System.Linq;

namespace MongoFramework.Tests.EntityRelationships.Mapping.EntityCollection
{
	[TestClass]
	public class CollectionMappingTests : DbTest
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
		public void ValidInversePropertyMapping()
		{
			var entityMapper = new EntityMapper<ValidInversePropertyModel>();
			var relationships = EntityMapperExtensions.GetEntityRelationships(entityMapper);

			Assert.IsTrue(relationships.Any(r => r.IsCollection && r.IdProperty.Name == "RelatedId"));
		}

		[TestMethod]
		[ExpectedException(typeof(MongoFrameworkMappingException))]
		public void InversePropertyMappingNonExistantProperty()
		{
			var entityMapper = new EntityMapper<InversePropertyNonExistantPropertyModel>();
			var relationships = EntityMapperExtensions.GetEntityRelationships(entityMapper);
		}

		[TestMethod]
		[ExpectedException(typeof(MongoFrameworkMappingException))]
		public void InversePropertyMappingInvalidPropertyType()
		{
			var entityMapper = new EntityMapper<InversePropertyMappingInvalidPropertyTypeModel>();
			var relationships = EntityMapperExtensions.GetEntityRelationships(entityMapper);
		}
	}
}
