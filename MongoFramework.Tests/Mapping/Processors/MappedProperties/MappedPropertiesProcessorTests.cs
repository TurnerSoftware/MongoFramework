using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mapping.Processors;

namespace MongoFramework.Tests.Mapping.Processors.MappedProperties
{
	public class NotMappedPropertiesModel
	{
		public string Id { get; set; }
		[NotMapped] public string NotMapped { get; set; }
	}

	public class ColumnAttributePropertyModel
	{
		public string Id { get; set; }
		[Column("CustomPropertyName")] public string MyProperty { get; set; }
	}

	[TestClass]
	public class MappedPropertiesProcessorTests
	{
		[TestMethod]
		public void ObeysNotMappedAttribute()
		{
			var processor = new MappedPropertiesProcessor();
			var classMap = new BsonClassMap<NotMappedPropertiesModel>();
			classMap.AutoMap();
			processor.ApplyMapping(typeof(NotMappedPropertiesModel), classMap);

			var entityMapper = new EntityMapper<NotMappedPropertiesModel>();
			var mappedProperties = entityMapper.GetEntityMapping();
			Assert.IsFalse(mappedProperties.Any(p => p.ElementName == "NotMapped"));
		}

		[TestMethod]
		public void ObeysColumnAttributeRemap()
		{
			var processor = new MappedPropertiesProcessor();
			var classMap = new BsonClassMap<ColumnAttributePropertyModel>();
			classMap.AutoMap();
			processor.ApplyMapping(typeof(ColumnAttributePropertyModel), classMap);

			var entityMapper = new EntityMapper<ColumnAttributePropertyModel>();
			var mappedProperties = entityMapper.GetEntityMapping();
			Assert.IsTrue(mappedProperties.Any(p => p.ElementName == "CustomPropertyName"));
		}
	}
}