using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson.Serialization;
using MongoFramework.Attributes;
using MongoFramework.Infrastructure.Mapping.Processors;
using MongoFramework.Infrastructure.Mapping.Serialization;
using System;
using System.Linq;

namespace MongoFramework.Tests.Mapping.Processors
{
	[TestClass]
	public class TypeDiscoveryProcessorTests
	{
		public class NoTypeDiscoveryAttributeModel
		{

		}

		[RuntimeTypeDiscovery]
		public class TypeDiscoveryAttributeModel
		{

		}

		[TestMethod]
		public void TypeDiscoverySerializerWhenAttributeIsDefined()
		{
			var processor = new TypeDiscoveryProcessor();
			var classMap = new BsonClassMap<TypeDiscoveryAttributeModel>();
			classMap.AutoMap();
			BsonClassMap.RegisterClassMap(classMap);

			processor.ApplyMapping(typeof(TypeDiscoveryAttributeModel), classMap);

			var serializer = BsonSerializer.LookupSerializer<TypeDiscoveryAttributeModel>();

			Assert.AreEqual(typeof(TypeDiscoverySerializer<>), serializer.GetType().GetGenericTypeDefinition());
		}

		[TestMethod]
		public void NotTypeDiscoverySerializerWhenAttributeNotDefined()
		{
			var processor = new TypeDiscoveryProcessor();
			var classMap = new BsonClassMap<NoTypeDiscoveryAttributeModel>();
			classMap.AutoMap();
			BsonClassMap.RegisterClassMap(classMap);

			processor.ApplyMapping(typeof(NoTypeDiscoveryAttributeModel), classMap);

			var serializer = BsonSerializer.LookupSerializer<NoTypeDiscoveryAttributeModel>();

			Assert.AreNotEqual(typeof(TypeDiscoverySerializer<>), serializer.GetType().GetGenericTypeDefinition());
		}
	}
}
