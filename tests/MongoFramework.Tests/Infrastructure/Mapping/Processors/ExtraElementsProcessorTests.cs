using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson.Serialization;
using MongoFramework.Attributes;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mapping.Processors;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MongoFramework.Tests.Infrastructure.Mapping.Processors
{
	[TestClass]
	public class ExtraElementsProcessorTests : MappingTestBase
	{
		[Table("ExtraElementsModel")]
		public class ExtraElementsAttrModel
		{
			public string Id { get; set; }
			[ExtraElements]
			public IDictionary<string, object> AdditionalElements { get; set; }
		}

		[Table("ExtraElementsModel")]
		public class ModelWithExtraElements
		{
			public string Id { get; set; }
			public string PropertyOne { get; set; }
			public int PropertyTwo { get; set; }
		}

		[IgnoreExtraElements]
		public class IgnoreExtraElementsModel
		{
			public string Id { get; set; }
		}

		[TestMethod]
		public void ObeysIgnoreExtraElementsAttribute()
		{
			EntityMapping.AddMappingProcessor(new ExtraElementsProcessor());
			EntityMapping.RegisterType(typeof(IgnoreExtraElementsModel));

			var classMap = BsonClassMap.GetRegisteredClassMaps()
				.Where(cm => cm.ClassType == typeof(IgnoreExtraElementsModel)).FirstOrDefault();
			Assert.IsTrue(classMap.IgnoreExtraElements);

			EntityMapping.RegisterType(typeof(ExtraElementsAttrModel));
			classMap = BsonClassMap.GetRegisteredClassMaps()
				.Where(cm => cm.ClassType == typeof(ExtraElementsAttrModel)).FirstOrDefault();
			Assert.IsFalse(classMap.IgnoreExtraElements);
		}

		[TestMethod]
		public void ObeysExtraElementsAttribute()
		{
			EntityMapping.AddMappingProcessor(new PropertyMappingProcessor());
			EntityMapping.AddMappingProcessor(new ExtraElementsProcessor());
			EntityMapping.RegisterType(typeof(ExtraElementsAttrModel));

			var classMap = BsonClassMap.GetRegisteredClassMaps()
				.Where(cm => cm.ClassType == typeof(ExtraElementsAttrModel)).FirstOrDefault();
			Assert.AreEqual("AdditionalElements", classMap.ExtraElementsMemberMap.ElementName);

			EntityMapping.RegisterType(typeof(IgnoreExtraElementsModel));
			classMap = BsonClassMap.GetRegisteredClassMaps()
				.Where(cm => cm.ClassType == typeof(IgnoreExtraElementsModel)).FirstOrDefault();
			Assert.AreEqual(null, classMap.ExtraElementsMemberMap);
		}

		[TestMethod]
		public void ExtraElementsSerializationIntegrationTest()
		{
			EntityMapping.AddMappingProcessor(new CollectionNameProcessor());
			EntityMapping.AddMappingProcessor(new PropertyMappingProcessor());
			EntityMapping.AddMappingProcessor(new ExtraElementsProcessor());
			EntityMapping.RegisterType(typeof(ExtraElementsAttrModel));
			EntityMapping.RegisterType(typeof(ModelWithExtraElements));

			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);

			var entity = new ModelWithExtraElements
			{
				PropertyOne = "ModelWithExtraElements",
				PropertyTwo = 123
			};
			context.ChangeTracker.SetEntityState(entity, EntityEntryState.Added);
			context.SaveChanges();

			var dbEntity = context.Query<ExtraElementsAttrModel>().Where(e => e.Id == entity.Id).FirstOrDefault();
			Assert.AreEqual("ModelWithExtraElements", dbEntity.AdditionalElements[nameof(ModelWithExtraElements.PropertyOne)]);
			Assert.AreEqual(123, dbEntity.AdditionalElements[nameof(ModelWithExtraElements.PropertyTwo)]);
		}
	}
}