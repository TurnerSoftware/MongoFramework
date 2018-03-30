using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mapping.Processors;
using MongoFramework.Tests.Models;
using System.Linq;

namespace MongoFramework.Tests.Mapping.Processors.MappedProperties
{
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