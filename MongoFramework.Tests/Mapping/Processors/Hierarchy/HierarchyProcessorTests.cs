using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mapping.Processors;
using MongoFramework.Tests.Models;

namespace MongoFramework.Tests.Mapping.Processors.Hierarchy
{
	[TestClass]
	public class HierarchyProcessorTests
	{
		[TestMethod]
		public void AccessToInherittedProperty()
		{
			var processor = new HierarchyProcessor();
			var classMap = new BsonClassMap<ChildTestModel>();
			processor.ApplyMapping(typeof(ChildTestModel), classMap);

			var entityMapper = new EntityMapper<ChildTestModel>();
			var mappedProperties = entityMapper.GetEntityMapping();
			Assert.IsTrue(mappedProperties.Any(p => p.ElementName == "Id"));
		}

		[TestMethod]
		public void AccessToDeclaredProperty()
		{
			var processor = new HierarchyProcessor();
			var classMap = new BsonClassMap<ChildTestModel>();
			processor.ApplyMapping(typeof(ChildTestModel), classMap);

			var entityMapper = new EntityMapper<ChildTestModel>();
			var mappedProperties = entityMapper.GetEntityMapping();
			Assert.IsTrue(mappedProperties.Any(p => p.ElementName == "DeclaredProperty"));
		}
	}
}