using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mapping.Processors;

namespace MongoFramework.Tests.Infrastructure.Mapping.Processors
{
	[TestClass]
	public class ClassMapPropertiesProcessorTests : MappingTestBase
	{
		public class BaseModel
		{
			public virtual string TestProperty { get => BaseProperty; set => BaseProperty = value; }
			public string BaseProperty { get; set; }
		}

		public class ChildModel : BaseModel
		{
			public override string TestProperty { get => ChildProperty; set => ChildProperty = value; }
			public string ChildProperty { get; set; }
		}

		[TestMethod]
		public void OverriddenPropertyDeserializationAppliesToChild()
		{
			EntityMapping.AddMappingProcessor(new ClassMapPropertiesProcessor());
			EntityMapping.RegisterType(typeof(ChildModel));

			var document = new BsonDocument
			{
				{ "_t", "ChildModel" },
				{ "TestProperty", "ChildDeserialization" }
			};

			var deserializedResult = BsonSerializer.Deserialize<ChildModel>(document);
			Assert.AreEqual("ChildDeserialization", deserializedResult.ChildProperty);
			Assert.IsNull(deserializedResult.BaseProperty);
		}

		[TestMethod]
		public void OverriddenPropertyDeserializationAppliesToBase()
		{
			EntityMapping.AddMappingProcessor(new ClassMapPropertiesProcessor());
			EntityMapping.RegisterType(typeof(ChildModel));

			var document = new BsonDocument
			{
				{ "_t", "BaseModel" },
				{ "TestProperty", "BaseDeserialization" }
			};

			var deserializedResult = BsonSerializer.Deserialize<BaseModel>(document);
			Assert.AreEqual("BaseDeserialization", deserializedResult.BaseProperty);
		}

		[TestMethod]
		public void DerivedTypeDeserializationAppliesToDiscriminatorType()
		{
			EntityMapping.AddMappingProcessor(new ClassMapPropertiesProcessor());
			EntityMapping.RegisterType(typeof(ChildModel));

			var document = new BsonDocument
			{
				{ "_t", "ChildModel" },
				{ "TestProperty", "ChildDeserialization" }
			};

			var deserializedResult = BsonSerializer.Deserialize<BaseModel>(document);
			Assert.AreEqual("ChildDeserialization", (deserializedResult as ChildModel).ChildProperty);
			Assert.IsNull(deserializedResult.BaseProperty);
		}
	}
}
