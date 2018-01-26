using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson.Serialization;
using MongoFramework.Attributes;
using MongoFramework.Infrastructure.Mapping.Processors;

namespace MongoFramework.Tests.Mapping.Processors.ExtraElements
{
	[IgnoreExtraElements]
	public class IgnoreExtraElementsModel
	{
		public string Id { get; set; }
	}

	public class ExtraElementsModel
	{
		public string Id { get; set; }
		[ExtraElements] public IDictionary<string, object> AdditionalElements { get; set; }
	}

	[TestClass]
	public class ExtraElementsProcessorTests
	{
		[TestMethod]
		public void ObeysIgnoreExtraElementsAttribute()
		{
			var processor = new ExtraElementsProcessor();
			var classMap = new BsonClassMap<IgnoreExtraElementsModel>();
			classMap.AutoMap();
			processor.ApplyMapping(typeof(IgnoreExtraElementsModel), classMap);

			Assert.IsTrue(classMap.IgnoreExtraElements);
		}

		[TestMethod]
		public void ObeysExtraElementsAttribute()
		{
			var processor = new ExtraElementsProcessor();
			var classMap = new BsonClassMap<ExtraElementsModel>();
			classMap.AutoMap();
			processor.ApplyMapping(typeof(ExtraElementsModel), classMap);

			Assert.AreEqual("AdditionalElements", classMap.ExtraElementsMemberMap?.ElementName);
		}
	}
}