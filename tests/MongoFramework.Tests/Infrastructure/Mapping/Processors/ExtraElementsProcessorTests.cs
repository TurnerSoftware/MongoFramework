using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson.Serialization;
using MongoFramework.Attributes;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mapping.Processors;
using System.Collections.Generic;
using System.Linq;

namespace MongoFramework.Tests.Infrastructure.Mapping.Processors
{
	[TestClass]
	public class ExtraElementsProcessorTests : MappingTestBase
	{
		public class ExtraElementsModel
		{
			public string Id { get; set; }
			[ExtraElements]
			public IDictionary<string, object> AdditionalElements { get; set; }
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

			EntityMapping.RegisterType(typeof(ExtraElementsModel));
			classMap = BsonClassMap.GetRegisteredClassMaps()
				.Where(cm => cm.ClassType == typeof(ExtraElementsModel)).FirstOrDefault();
			Assert.IsFalse(classMap.IgnoreExtraElements);
		}

		[TestMethod]
		public void ObeysExtraElementsAttribute()
		{
			EntityMapping.AddMappingProcessor(new ExtraElementsProcessor());
			EntityMapping.RegisterType(typeof(ExtraElementsModel));

			var classMap = BsonClassMap.GetRegisteredClassMaps()
				.Where(cm => cm.ClassType == typeof(ExtraElementsModel)).FirstOrDefault();
			Assert.AreEqual("AdditionalElements", classMap.ExtraElementsMemberMap.ElementName);

			EntityMapping.RegisterType(typeof(IgnoreExtraElementsModel));
			classMap = BsonClassMap.GetRegisteredClassMaps()
				.Where(cm => cm.ClassType == typeof(IgnoreExtraElementsModel)).FirstOrDefault();
			Assert.AreEqual(null, classMap.ExtraElementsMemberMap);
		}
	}
}