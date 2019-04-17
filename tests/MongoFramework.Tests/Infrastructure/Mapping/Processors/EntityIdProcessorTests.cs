using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoFramework.Infrastructure.Mapping.Processors;
using System;
using System.ComponentModel.DataAnnotations;

namespace MongoFramework.Tests.Infrastructure.Mapping.Processors
{
	[TestClass]
	public class EntityIdProcessorTests : TestBase
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

		[TestMethod]
		public void IdMapsOnAttribute()
		{
			var connection = TestConfiguration.GetConnection();
			var processor = new EntityIdProcessor();
			var classMap = new BsonClassMap<IdByAttributeTestModel>();
			processor.ApplyMapping(typeof(IdByAttributeTestModel), classMap, connection);

			var entityMapper = connection.GetEntityMapper(typeof(IdByAttributeTestModel));
			Assert.AreEqual("MyCustomId", entityMapper.GetIdName());
		}

		[TestMethod]
		public void StringIdGeneratorOnStringProperty()
		{
			var connection = TestConfiguration.GetConnection();
			var processor = new EntityIdProcessor();
			var classMap = new BsonClassMap<StringIdGeneratorTestModel>();
			processor.ApplyMapping(typeof(StringIdGeneratorTestModel), classMap, connection);

			Assert.AreEqual(typeof(StringObjectIdGenerator), classMap.IdMemberMap.IdGenerator?.GetType());
		}

		[TestMethod]
		public void GuidIdGeneratorOnGuidProperty()
		{
			var connection = TestConfiguration.GetConnection();
			var processor = new EntityIdProcessor();
			var classMap = new BsonClassMap<GuidIdGeneratorTestModel>();
			processor.ApplyMapping(typeof(GuidIdGeneratorTestModel), classMap, connection);

			Assert.AreEqual(typeof(CombGuidGenerator), classMap.IdMemberMap.IdGenerator?.GetType());
		}

		[TestMethod]
		public void ObjectIdGeneratorOnObjectIdProperty()
		{
			var connection = TestConfiguration.GetConnection();
			var processor = new EntityIdProcessor();
			var classMap = new BsonClassMap<ObjectIdGeneratorTestModel>();
			processor.ApplyMapping(typeof(ObjectIdGeneratorTestModel), classMap, connection);

			Assert.AreEqual(typeof(ObjectIdGenerator), classMap.IdMemberMap.IdGenerator?.GetType());
		}
	}
}