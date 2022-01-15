using System;
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
			EntityMapping.RegisterType(typeof(StringIdGeneratorTestModel));

			var classMap = BsonClassMap.GetRegisteredClassMaps()
				.Where(cm => cm.ClassType == typeof(StringIdGeneratorTestModel)).FirstOrDefault();

			Assert.AreEqual(typeof(StringObjectIdGenerator), classMap.IdMemberMap.IdGenerator?.GetType());
		}

		[TestMethod]
		public void GuidIdGeneratorOnGuidProperty()
		{
			EntityMapping.AddMappingProcessor(new PropertyMappingProcessor());
			EntityMapping.AddMappingProcessor(new EntityIdProcessor());
			EntityMapping.RegisterType(typeof(GuidIdGeneratorTestModel));

			var classMap = BsonClassMap.GetRegisteredClassMaps()
				.Where(cm => cm.ClassType == typeof(GuidIdGeneratorTestModel)).FirstOrDefault();

			Assert.AreEqual(typeof(CombGuidGenerator), classMap.IdMemberMap.IdGenerator?.GetType());
		}

		[TestMethod]
		public void ObjectIdGeneratorOnObjectIdProperty()
		{
			EntityMapping.AddMappingProcessor(new PropertyMappingProcessor());
			EntityMapping.AddMappingProcessor(new EntityIdProcessor());
			EntityMapping.RegisterType(typeof(ObjectIdGeneratorTestModel));

			var classMap = BsonClassMap.GetRegisteredClassMaps()
				.Where(cm => cm.ClassType == typeof(ObjectIdGeneratorTestModel)).FirstOrDefault();

			Assert.AreEqual(typeof(ObjectIdGenerator), classMap.IdMemberMap.IdGenerator?.GetType());
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