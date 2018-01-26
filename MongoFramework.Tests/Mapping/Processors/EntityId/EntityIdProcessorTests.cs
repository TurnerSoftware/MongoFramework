using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mapping.Processors;

namespace MongoFramework.Tests.Mapping.Processors.EntityId
{
	public class IdByAttributeTestModel
	{
		[Key]
		public string MyCustomId { get; set; }
	}

	public class StringIdGeneratorTestModel
	{
		[Key]
		public string MyCustomId { get; set; }
	}

	public class GuidIdGeneratorTestModel
	{
		[Key]
		public Guid MyCustomId { get; set; }
	}

	public class ObjectIdGeneratorTestModel
	{
		[Key]
		public ObjectId MyCustomId { get; set; }
	}

	[TestClass]
	public class EntityIdProcessorTests
	{
		[TestMethod]
		public void IdMapsOnAttribute()
		{
			var processor = new EntityIdProcessor();
			var classMap = new BsonClassMap<IdByAttributeTestModel>();
			processor.ApplyMapping(typeof(IdByAttributeTestModel), classMap);

			var entityMapper = new EntityMapper<IdByAttributeTestModel>();
			Assert.AreEqual("MyCustomId", entityMapper.GetIdName());
		}

		[TestMethod]
		public void StringIdGeneratorOnStringProperty()
		{
			var processor = new EntityIdProcessor();
			var classMap = new BsonClassMap<StringIdGeneratorTestModel>();
			processor.ApplyMapping(typeof(StringIdGeneratorTestModel), classMap);

			Assert.AreEqual(typeof(StringObjectIdGenerator), classMap.IdMemberMap.IdGenerator?.GetType());
		}

		[TestMethod]
		public void GuidIdGeneratorOnGuidProperty()
		{
			var processor = new EntityIdProcessor();
			var classMap = new BsonClassMap<GuidIdGeneratorTestModel>();
			processor.ApplyMapping(typeof(GuidIdGeneratorTestModel), classMap);

			Assert.AreEqual(typeof(CombGuidGenerator), classMap.IdMemberMap.IdGenerator?.GetType());
		}

		[TestMethod]
		public void ObjectIdGeneratorOnObjectIdProperty()
		{
			var processor = new EntityIdProcessor();
			var classMap = new BsonClassMap<ObjectIdGeneratorTestModel>();
			processor.ApplyMapping(typeof(ObjectIdGeneratorTestModel), classMap);

			Assert.AreEqual(typeof(ObjectIdGenerator), classMap.IdMemberMap.IdGenerator?.GetType());
		}
	}
}