using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure.Mapping.Processors;
using MongoFramework.Tests.Models;

namespace MongoFramework.Tests.Mapping.Processors.NestedProperty
{
	[TestClass]
	public class NestedPropertyProcessorTests
	{
		[TestMethod]
		public void MapsSimpleNestedProperty()
		{
			var processor = new NestedPropertyProcessor();
			var classMap = new BsonClassMap<ParentModel>();
			classMap.AutoMap();

			var registeredClassMaps = BsonClassMap.GetRegisteredClassMaps();
			Assert.IsFalse(registeredClassMaps.Any(m => m.ClassType == typeof(NestedModel)));

			processor.ApplyMapping(typeof(ParentModel), classMap);

			registeredClassMaps = BsonClassMap.GetRegisteredClassMaps();
			Assert.IsTrue(registeredClassMaps.Any(m => m.ClassType == typeof(NestedModel)));
		}
	}
}