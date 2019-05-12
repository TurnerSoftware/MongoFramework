using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mapping.Processors;
using System.Collections.Generic;
using System.Linq;

namespace MongoFramework.Tests.Infrastructure.Mapping.Processors
{
	[TestClass]
	public class NestedPropertyProcessorTests : MappingTestBase
	{
		public class CollectionBaseModel
		{
			public ICollection<CollectionNestedModel> CollectionModel { get; set; }
		}

		public class CollectionNestedModel
		{
			public string HelloWorld { get; set; }
		}

		public class PropertyBaseModel
		{
			public PropertyNestedModel Model { get; set; }
		}

		public class PropertyNestedModel
		{
			public string HelloWorld { get; set; }
		}

		[TestMethod]
		public void MapsNestedStandardPropertyModel()
		{
			EntityMapping.AddMappingProcessor(new NestedPropertyProcessor());
			Assert.IsFalse(BsonClassMap.IsClassMapRegistered(typeof(PropertyNestedModel)));
			EntityMapping.RegisterType(typeof(PropertyBaseModel));
			Assert.IsTrue(BsonClassMap.IsClassMapRegistered(typeof(PropertyNestedModel)));
		}

		[TestMethod]
		public void MapsNestedCollectionPropertyModel()
		{
			EntityMapping.AddMappingProcessor(new NestedPropertyProcessor());
			Assert.IsFalse(BsonClassMap.IsClassMapRegistered(typeof(CollectionNestedModel)));
			EntityMapping.RegisterType(typeof(CollectionBaseModel));
			Assert.IsTrue(BsonClassMap.IsClassMapRegistered(typeof(CollectionNestedModel)));
		}
	}
}