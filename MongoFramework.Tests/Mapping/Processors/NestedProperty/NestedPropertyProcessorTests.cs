using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure.Mapping.Processors;
using MongoFramework.Tests.Models;
using System.Linq;

namespace MongoFramework.Tests.Mapping.Processors.NestedProperty
{
	[TestClass]
	public class NestedPropertyProcessorTests
	{
		[TestMethod]
		public void MapsNestedStandardPropertyModel()
		{
			var processor = new NestedPropertyProcessor();
			var classMap = new BsonClassMap<PropertyBaseModel>();
			classMap.AutoMap();

			var registeredClassMaps = BsonClassMap.GetRegisteredClassMaps();
			Assert.IsFalse(registeredClassMaps.Any(m => m.ClassType == typeof(PropertyNestedModel)));

			processor.ApplyMapping(typeof(PropertyBaseModel), classMap);

			registeredClassMaps = BsonClassMap.GetRegisteredClassMaps();
			Assert.IsTrue(registeredClassMaps.Any(m => m.ClassType == typeof(PropertyNestedModel)));
		}

		[TestMethod]
		public void MapsNestedCollectionPropertyModel()
		{
			var processor = new NestedPropertyProcessor();
			var classMap = new BsonClassMap<CollectionBaseModel>();
			classMap.AutoMap();

			var registeredClassMaps = BsonClassMap.GetRegisteredClassMaps();
			Assert.IsFalse(registeredClassMaps.Any(m => m.ClassType == typeof(CollectionNestedModel)));

			processor.ApplyMapping(typeof(CollectionBaseModel), classMap);

			registeredClassMaps = BsonClassMap.GetRegisteredClassMaps();
			Assert.IsTrue(registeredClassMaps.Any(m => m.ClassType == typeof(CollectionNestedModel)));
		}
	}
}