﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mapping.Processors;

namespace MongoFramework.Tests.Infrastructure.Mapping.Processors
{
	[TestClass]
	public class EntityIdProcessorTests : MappingTestBase
	{
		public class GuidIdGeneratorTestModel
		{
			[Key]
			public Guid MyCustomId { get; set; }
		}

		public class IdByAttributeTestModel
		{
			[Key]
			public string MyCustomId { get; set; }
		}

		public class ObjectIdGeneratorTestModel
		{
			[Key]
			public ObjectId MyCustomId { get; set; }
		}

		public class StringIdGeneratorTestModel
		{
			[Key]
			public string MyCustomId { get; set; }
		}

		public class ExplicitKeyOverridesImplicitIdModel
		{
			public string Id { get; set; }
			[Key]
			public string ActualKey { get; set; }
		}

		[TestMethod]
		public void IdMapsOnAttribute()
		{
			EntityMapping.AddMappingProcessor(new PropertyMappingProcessor());
			EntityMapping.AddMappingProcessor(new EntityIdProcessor());
			var definition = EntityMapping.RegisterType(typeof(IdByAttributeTestModel));
			Assert.AreEqual("MyCustomId", definition.GetIdName());
		}

		[TestMethod]
		public void StringIdGeneratorOnStringProperty()
		{
			EntityMapping.AddMappingProcessor(new PropertyMappingProcessor());
			EntityMapping.AddMappingProcessor(new EntityIdProcessor());
			var definition = EntityMapping.RegisterType(typeof(StringIdGeneratorTestModel));

			Assert.AreEqual(EntityKeyGenerators.StringKeyGenerator, definition.Key.KeyGenerator);
		}

		[TestMethod]
		public void GuidIdGeneratorOnGuidProperty()
		{
			EntityMapping.AddMappingProcessor(new PropertyMappingProcessor());
			EntityMapping.AddMappingProcessor(new EntityIdProcessor());
			var definition = EntityMapping.RegisterType(typeof(GuidIdGeneratorTestModel));

			Assert.AreEqual(EntityKeyGenerators.GuidKeyGenerator, definition.Key.KeyGenerator);
		}

		[TestMethod]
		public void ObjectIdGeneratorOnObjectIdProperty()
		{
			EntityMapping.AddMappingProcessor(new PropertyMappingProcessor());
			EntityMapping.AddMappingProcessor(new EntityIdProcessor());
			var definition = EntityMapping.RegisterType(typeof(ObjectIdGeneratorTestModel));

			Assert.AreEqual(EntityKeyGenerators.ObjectIdKeyGenerator, definition.Key.KeyGenerator);
		}

		[TestMethod]
		public void ExplicitKeyOverridesImplicitId()
		{
			EntityMapping.AddMappingProcessor(new PropertyMappingProcessor());
			EntityMapping.AddMappingProcessor(new EntityIdProcessor());
			EntityMapping.RegisterType(typeof(ExplicitKeyOverridesImplicitIdModel));

			var classMap = BsonClassMap.GetRegisteredClassMaps()
				.Where(cm => cm.ClassType == typeof(ExplicitKeyOverridesImplicitIdModel)).FirstOrDefault();

			Assert.AreEqual("ActualKey", classMap.IdMemberMap.MemberName);
		}
	}
}