using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson.Serialization;
using MongoFramework.Attributes;
using MongoFramework.Infrastructure.Mapping.Processors;
using System.Collections.Generic;

namespace MongoFramework.Tests.Infrastructure.Mapping.Processors
{
	[TestClass]
	public class ExtraElementsProcessorTests : TestBase
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
			var connection = TestConfiguration.GetConnection();
			var processor = new ExtraElementsProcessor();
			var classMap = new BsonClassMap<IgnoreExtraElementsModel>();
			classMap.AutoMap();
			processor.ApplyMapping(typeof(IgnoreExtraElementsModel), classMap, connection);

			Assert.IsTrue(classMap.IgnoreExtraElements);
		}

		[TestMethod]
		public void ObeysExtraElementsAttribute()
		{
			var connection = TestConfiguration.GetConnection();
			var processor = new ExtraElementsProcessor();
			var classMap = new BsonClassMap<ExtraElementsModel>();
			classMap.AutoMap();
			processor.ApplyMapping(typeof(ExtraElementsModel), classMap, connection);

			Assert.AreEqual("AdditionalElements", classMap.ExtraElementsMemberMap?.ElementName);
		}
	}
}