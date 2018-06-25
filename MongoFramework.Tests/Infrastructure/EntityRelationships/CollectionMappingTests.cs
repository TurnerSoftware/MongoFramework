﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure.EntityRelationships;
using MongoFramework.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MongoFramework.Tests.Infrastructure.EntityRelationships
{
	[TestClass]
	public class CollectionMappingTests : TestBase
	{
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
