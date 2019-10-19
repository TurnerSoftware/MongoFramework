using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mapping.Processors;
using System.Linq;

namespace MongoFramework.Tests.Infrastructure.Mapping.Processors
{
	[TestClass]
	public class HierarchyProcessorTests : MappingTestBase
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
		public void ParentClassIsMapped()
		{
			EntityMapping.AddMappingProcessor(new HierarchyProcessor());

			Assert.IsFalse(BsonClassMap.IsClassMapRegistered(typeof(ChildTestModel)));
			Assert.IsFalse(BsonClassMap.IsClassMapRegistered(typeof(ParentTestModel)));

			EntityMapping.RegisterType(typeof(ChildTestModel));

			Assert.IsTrue(BsonClassMap.IsClassMapRegistered(typeof(ChildTestModel)));
			Assert.IsTrue(BsonClassMap.IsClassMapRegistered(typeof(ParentTestModel)));
		}

		[TestMethod]
		public void AccessToInherittedProperty()
		{
			EntityMapping.AddMappingProcessor(new HierarchyProcessor());
			EntityMapping.AddMappingProcessor(new PropertyMappingProcessor());

			var definition = EntityMapping.RegisterType(typeof(ChildTestModel));

			var allProperties = definition.GetAllProperties();
			Assert.IsTrue(allProperties.Any(p => p.ElementName == "Id"));

			var declaredProperties = definition.Properties;
			Assert.IsFalse(declaredProperties.Any(p => p.ElementName == "Id"));
		}

		[TestMethod]
		public void AccessToDeclaredProperty()
		{
			EntityMapping.AddMappingProcessor(new HierarchyProcessor());
			EntityMapping.AddMappingProcessor(new PropertyMappingProcessor());
			var definition = EntityMapping.RegisterType(typeof(ChildTestModel));

			var mappedProperties = definition.GetAllProperties();
			Assert.IsTrue(mappedProperties.Any(p => p.ElementName == "DeclaredProperty"));

			var inherittedProperties = definition.GetInheritedProperties();
			Assert.IsFalse(inherittedProperties.Any(p => p.ElementName == "DeclaredProperty"));
		}
	}
}