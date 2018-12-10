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
			var connection = TestConfiguration.GetConnection();
			var entityMapper = connection.GetEntityMapper(typeof(BaseEntityModel));
			var relationships = EntityMapperExtensions.GetEntityRelationships(entityMapper, connection);

			var createdByIdProperty = typeof(BaseEntityModel).GetProperty("CreatedById");
			var attributeOnIdRelationship = relationships.Where(r => r.IdProperty == createdByIdProperty).FirstOrDefault();

			Assert.IsFalse(attributeOnIdRelationship.IsCollection);
			Assert.AreEqual(typeof(UserEntityModel), attributeOnIdRelationship.EntityType);
			Assert.AreEqual(typeof(BaseEntityModel).GetProperty("CreatedBy"), attributeOnIdRelationship.NavigationProperty);
		}

		[TestMethod]
		public void ForeignKeyAttributeOnNavigationProperty()
		{
			var connection = TestConfiguration.GetConnection();
			var entityMapper = connection.GetEntityMapper(typeof(BaseEntityModel));
			var relationships = EntityMapperExtensions.GetEntityRelationships(entityMapper, connection);

			var updatedByIdProperty = typeof(BaseEntityModel).GetProperty("UpdatedById");
			var attributeOnIdRelationship = relationships.Where(r => r.IdProperty == updatedByIdProperty).FirstOrDefault();

			Assert.IsFalse(attributeOnIdRelationship.IsCollection);
			Assert.AreEqual(typeof(UserEntityModel), attributeOnIdRelationship.EntityType);
			Assert.AreEqual(typeof(BaseEntityModel).GetProperty("UpdatedBy"), attributeOnIdRelationship.NavigationProperty);
		}

		[TestMethod]
		public void IdentifyRelationshipsWithOtherIdTypes()
		{
			var connection = TestConfiguration.GetConnection();
			var entityMapper = connection.GetEntityMapper(typeof(BaseVariedIdModel));
			var relationships = EntityMapperExtensions.GetEntityRelationships(entityMapper, connection);
			Assert.AreEqual(2, relationships.Count());
		}

		[TestMethod]
		[ExpectedExceptionPattern(typeof(InvalidOperationException), @"Unable to determine the Id property between .+ and .+\. Check the types for these properties are correct\.")]
		public void UnsupportedIdTypeOnRelationship()
		{
			var connection = TestConfiguration.GetConnection();
			var entityMapper = connection.GetEntityMapper(typeof(UnsupportedIdModel));
			var relationships = EntityMapperExtensions.GetEntityRelationships(entityMapper, connection).ToArray();
		}

		[TestMethod]
		[ExpectedExceptionPattern(typeof(InvalidOperationException), @"Can't find property .+ in .+ as indicated by the ForeignKeyAttribute.")]
		public void InvalidForeignKeyOnRelationship()
		{
			var connection = TestConfiguration.GetConnection();
			var entityMapper = connection.GetEntityMapper(typeof(InvalidForeignKeyModel));
			var relationships = EntityMapperExtensions.GetEntityRelationships(entityMapper, connection).ToArray();
		}

		[TestMethod]
		public void NavigationPropertiesUnmap()
		{
			var connection = TestConfiguration.GetConnection();
			var entityMapper = connection.GetEntityMapper(typeof(BaseEntityModel));
			Assert.IsFalse(entityMapper.GetEntityMapping().Any(e => e.FullPath == "CreatedBy" || e.FullPath == "UpdatedBy"));
		}
	}
}
