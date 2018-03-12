using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure.Mapping.Processors;
using MongoFramework.Tests.Models;

namespace MongoFramework.Tests.Mapping.Processors.ExtraElements
{
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