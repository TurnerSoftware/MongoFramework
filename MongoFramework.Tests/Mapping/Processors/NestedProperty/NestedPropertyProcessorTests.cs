using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure.Mapping.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Tests.Mapping.Processors.NestedProperty
{
	public class ParentModel
	{
		public string Id { get; set; }
		public NestedModel Model { get; set; }
	}

	public class NestedModel
	{
		public string HelloWorld { get; set; }
	}

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
