using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mapping.Processors;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MongoFramework.Tests.Infrastructure.Mapping.Processors
{
	[TestClass]
	public class PropertyMappingProcessorTests : MappingTestBase
	{
		public class ColumnAttributePropertyModel
		{
			public string Id { get; set; }
			[Column("CustomPropertyName")]
			public string MyProperty { get; set; }
		}

		public class NotMappedPropertiesModel
		{
			public string Id { get; set; }
			[NotMapped]
			public string NotMapped { get; set; }
		}
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
		public void ObeysNotMappedAttribute()
		{
			EntityMapping.AddMappingProcessor(new PropertyMappingProcessor());
			var definition = EntityMapping.RegisterType(typeof(NotMappedPropertiesModel));
			Assert.IsFalse(definition.Properties.Any(p => p.ElementName == "NotMapped"));
		}

		[TestMethod]
		public void ObeysColumnAttributeRemap()
		{
			EntityMapping.AddMappingProcessor(new PropertyMappingProcessor());
			var definition = EntityMapping.RegisterType(typeof(ColumnAttributePropertyModel));
			Assert.IsTrue(definition.Properties.Any(p => p.ElementName == "CustomPropertyName"));
		}

		[TestMethod]
		public void OverriddenPropertyDeserializationAppliesToChild()
		{
			EntityMapping.AddMappingProcessor(new PropertyMappingProcessor());
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
			EntityMapping.AddMappingProcessor(new PropertyMappingProcessor());
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
			EntityMapping.AddMappingProcessor(new PropertyMappingProcessor());
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