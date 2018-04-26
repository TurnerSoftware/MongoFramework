using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoFramework.Infrastructure.EntityRelationships;
using MongoFramework.Infrastructure.Mapping;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MongoFramework.Tests.Infrastructure.EntityRelationships
{
	[TestClass]
	public class SingleEntityMappingTests : TestBase
	{
		public class BaseEntityModel
		{
			public string Id { get; set; }
			public string Description { get; set; }

			[ForeignKey("CreatedBy")]
			public string CreatedById { get; set; }
			public virtual UserEntityModel CreatedBy { get; set; }

			public string UpdatedById { get; set; }
			[ForeignKey("UpdatedById")]
			public virtual UserEntityModel UpdatedBy { get; set; }
		}

		public class BaseVariedIdModel
		{
			public string Id { get; set; }

			[ForeignKey("GuidProperty")]
			public Guid GuidTestId { get; set; }
			public virtual GuidIdModel GuidProperty { get; set; }

			[ForeignKey("ObjectIdProperty")]
			public ObjectId ObjectIdTestId { get; set; }
			public virtual ObjectIdIdModel ObjectIdProperty { get; set; }
		}

		public class InvalidForeignKeyModel
		{
			public string Id { get; set; }

			[ForeignKey("Created_By")]
			public string CreatedById { get; set; }
			public virtual UserEntityModel CreatedBy { get; set; }
		}

		public class UnsupportedIdModel
		{
			public string Id { get; set; }

			[ForeignKey("CreatedBy")]
			public int CreatedById { get; set; }
			public virtual UserEntityModel CreatedBy { get; set; }
		}

		public class UserEntityModel
		{
			public string Id { get; set; }
			public string Username { get; set; }
		}

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
