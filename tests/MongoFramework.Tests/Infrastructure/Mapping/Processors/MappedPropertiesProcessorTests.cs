using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mapping.Processors;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MongoFramework.Tests.Infrastructure.Mapping.Processors
{
	[TestClass]
	public class MappedPropertiesProcessorTests : MappingTestBase
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

		[TestMethod]
		public void ObeysNotMappedAttribute()
		{
			EntityMapping.AddMappingProcessor(new MappedPropertiesProcessor());
			EntityMapping.AddMappingProcessor(new ClassMapPropertiesProcessor());
			var definition = EntityMapping.RegisterType(typeof(NotMappedPropertiesModel));
			Assert.IsFalse(definition.Properties.Any(p => p.ElementName == "NotMapped"));
		}

		[TestMethod]
		public void ObeysColumnAttributeRemap()
		{
			EntityMapping.AddMappingProcessor(new MappedPropertiesProcessor());
			EntityMapping.AddMappingProcessor(new ClassMapPropertiesProcessor());
			var definition = EntityMapping.RegisterType(typeof(ColumnAttributePropertyModel));
			Assert.IsTrue(definition.Properties.Any(p => p.ElementName == "CustomPropertyName"));
		}
	}
}