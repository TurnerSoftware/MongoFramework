using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mapping.Processors;
using System.Linq;

namespace MongoFramework.Tests.Infrastructure.Mapping.Processors
{
	[TestClass]
	public class HierarchyProcessorTests : TestBase
	{
		public class ParentTestModel
		{
			public string Id { get; set; }
		}

		public class ChildTestModel : ParentTestModel
		{
			public string DeclaredProperty { get; set; }
		}

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