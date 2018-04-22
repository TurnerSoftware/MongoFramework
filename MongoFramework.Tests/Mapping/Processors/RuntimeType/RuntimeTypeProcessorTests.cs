using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure.Mapping.Processors;
using MongoFramework.Infrastructure.Mapping.Serialization;

namespace MongoFramework.Tests.Mapping.Processors.RuntimeType
{
	[TestClass]
	public class RuntimeTypeProcessorTests
	{
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
