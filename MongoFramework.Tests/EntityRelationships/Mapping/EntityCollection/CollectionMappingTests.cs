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
			var entityMapper = new EntityMapper<CollectionMappingModel>();
			var relationships = EntityMapperExtensions.GetEntityRelationships(entityMapper);

			Assert.IsTrue(relationships.All(r => r.IsCollection));

			var relationship = relationships.Where(r => r.NavigationProperty.Name == "StringModelEntities").FirstOrDefault();

			Assert.IsTrue(relationship.IsCollection);
			Assert.AreEqual(typeof(StringIdModel), relationship.EntityType);
			Assert.AreEqual(typeof(StringIdModel).GetProperty("Id"), relationship.IdProperty);
			Assert.AreEqual(typeof(CollectionMappingModel).GetProperty("StringModelEntities"), relationship.NavigationProperty);
		}

		[TestMethod]
		public void ValidInversePropertyMapping()
		{
			var entityMapper = new EntityMapper<ValidInversePropertyModel>();
			var relationships = EntityMapperExtensions.GetEntityRelationships(entityMapper);

			Assert.IsTrue(relationships.Any(r => r.IsCollection && r.IdProperty.Name == "SecondaryId"));
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
