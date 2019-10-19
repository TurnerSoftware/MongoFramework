using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mapping.Processors;
using MongoFramework.Tests.Infrastructure.EntityRelationships;

namespace MongoFramework.Tests.Infrastructure.Mapping.Processors
{
	[TestClass]
	public class EntityRelationshipProcessorTests : MappingTestBase
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

		public class CollectionMappingModel
		{
			public string Id { get; set; }
			public string Description { get; set; }

			public virtual ICollection<StringIdModel> StringModelEntities { get; set; }
			public virtual ICollection<ObjectIdIdModel> ObjectIdModelEntities { get; set; }
			public virtual ICollection<GuidIdModel> GuidModelEntities { get; set; }

			[InverseProperty("SecondaryId")]
			public virtual ICollection<StringIdModel> InverseCollection { get; set; }
		}

		public class InversePropertyMappingInvalidPropertyTypeModel
		{
			public string Id { get; set; }

			[InverseProperty("CreatedDate")]
			public virtual ICollection<StringIdModel> StringModelEntities { get; set; }
		}

		public class InversePropertyNonExistantPropertyModel
		{
			public string Id { get; set; }

			[InverseProperty("NonExistantPropertyId")]
			public virtual ICollection<StringIdModel> StringModelEntities { get; set; }
		}

		public class ValidInversePropertyModel
		{
			public string Id { get; set; }

			[InverseProperty("SecondaryId")]
			public virtual ICollection<StringIdModel> StringModelEntities { get; set; }
		}


		[TestMethod]
		public void ForeignKeyAttributeOnId()
		{
			EntityMapping.AddMappingProcessor(new PropertyMappingProcessor());
			EntityMapping.AddMappingProcessor(new EntityRelationshipProcessor());

			var definition = EntityMapping.RegisterType(typeof(BaseEntityModel));
			var relationships = definition.Relationships;

			var attributeOnIdRelationship = relationships.Where(r => r.IdProperty.ElementName == "CreatedById").FirstOrDefault();

			Assert.IsFalse(attributeOnIdRelationship.IsCollection);
			Assert.AreEqual(typeof(UserEntityModel), attributeOnIdRelationship.EntityType);
			Assert.AreEqual("CreatedBy", attributeOnIdRelationship.NavigationProperty.ElementName);
		}

		[TestMethod]
		public void ForeignKeyAttributeOnNavigationProperty()
		{
			EntityMapping.AddMappingProcessor(new PropertyMappingProcessor());
			EntityMapping.AddMappingProcessor(new EntityRelationshipProcessor());

			var definition = EntityMapping.RegisterType(typeof(BaseEntityModel));
			var relationships = definition.Relationships;

			var attributeOnIdRelationship = relationships.Where(r => r.IdProperty.ElementName == "UpdatedById").FirstOrDefault();

			Assert.IsFalse(attributeOnIdRelationship.IsCollection);
			Assert.AreEqual(typeof(UserEntityModel), attributeOnIdRelationship.EntityType);
			Assert.AreEqual("UpdatedBy", attributeOnIdRelationship.NavigationProperty.ElementName);
		}

		[TestMethod]
		public void IdentifyRelationshipsWithOtherIdTypes()
		{
			EntityMapping.AddMappingProcessor(new PropertyMappingProcessor());
			EntityMapping.AddMappingProcessor(new EntityRelationshipProcessor());

			var relationships = EntityMapping.RegisterType(typeof(BaseVariedIdModel)).Relationships;
			Assert.AreEqual(2, relationships.Count());
		}

		[TestMethod]
		[ExpectedExceptionPattern(typeof(InvalidOperationException), @"Unable to determine the Id property between .+ and .+\. Check the types for these properties are valid\.")]
		public void UnsupportedIdTypeOnRelationship()
		{
			EntityMapping.AddMappingProcessor(new PropertyMappingProcessor());
			EntityMapping.AddMappingProcessor(new EntityRelationshipProcessor());

			EntityMapping.RegisterType(typeof(UnsupportedIdModel));
		}

		[TestMethod]
		[ExpectedExceptionPattern(typeof(InvalidOperationException), @"Can't find property .+ in .+ as indicated by the ForeignKeyAttribute.")]
		public void InvalidForeignKeyOnRelationship()
		{
			EntityMapping.AddMappingProcessor(new PropertyMappingProcessor());
			EntityMapping.AddMappingProcessor(new EntityRelationshipProcessor());

			EntityMapping.RegisterType(typeof(InvalidForeignKeyModel));
		}

		[TestMethod]
		public void NavigationPropertiesUnmap()
		{
			EntityMapping.AddMappingProcessor(new PropertyMappingProcessor());
			EntityMapping.AddMappingProcessor(new EntityRelationshipProcessor());

			var definition = EntityMapping.RegisterType(typeof(BaseEntityModel));

			Assert.IsFalse(definition.Properties.Any(e => e.FullPath == "CreatedBy" || e.FullPath == "UpdatedBy"));
		}

		[TestMethod]
		public void IdentifyCollectionRelationships()
		{
			EntityMapping.AddMappingProcessor(new PropertyMappingProcessor());
			EntityMapping.AddMappingProcessor(new EntityRelationshipProcessor());

			var definition = EntityMapping.RegisterType(typeof(CollectionMappingModel));
			var relationships = definition.Relationships;

			Assert.IsTrue(relationships.All(r => r.IsCollection));

			var relationship = relationships.Where(r => r.NavigationProperty.PropertyInfo.Name == "StringModelEntities").FirstOrDefault();
			var stringIdModelDefinition = EntityMapping.GetOrCreateDefinition(typeof(StringIdModel));

			Assert.IsTrue(relationship.IsCollection);
			Assert.AreEqual(typeof(StringIdModel), relationship.EntityType);
			Assert.IsTrue(stringIdModelDefinition.GetProperty("Id").Equals(relationship.IdProperty));
			Assert.IsTrue(definition.GetProperty("StringModelEntities").Equals(relationship.NavigationProperty));
		}

		[TestMethod]
		public void ValidInversePropertyMapping()
		{
			EntityMapping.AddMappingProcessor(new PropertyMappingProcessor());
			EntityMapping.AddMappingProcessor(new EntityRelationshipProcessor());

			var relationships = EntityMapping.RegisterType(typeof(ValidInversePropertyModel)).Relationships;

			Assert.IsTrue(relationships.Any(r => r.IsCollection && r.IdProperty.PropertyInfo.Name == "SecondaryId"));
		}

		[TestMethod]
		[ExpectedExceptionPattern(typeof(InvalidOperationException), "Can't find property .+ in .+ as indicated by the InversePropertyAttribute on .+")]
		public void InversePropertyMappingNonExistantProperty()
		{
			EntityMapping.AddMappingProcessor(new PropertyMappingProcessor());
			EntityMapping.AddMappingProcessor(new EntityRelationshipProcessor());

			EntityMapping.RegisterType(typeof(InversePropertyNonExistantPropertyModel));
		}

		[TestMethod]
		[ExpectedExceptionPattern(typeof(InvalidOperationException), "Can't find property .+ in .+ as indicated by the InversePropertyAttribute on .+")]
		public void InversePropertyMappingInvalidPropertyType()
		{
			EntityMapping.AddMappingProcessor(new PropertyMappingProcessor());
			EntityMapping.AddMappingProcessor(new EntityRelationshipProcessor());

			EntityMapping.RegisterType(typeof(InversePropertyMappingInvalidPropertyTypeModel));
		}
	}
}
