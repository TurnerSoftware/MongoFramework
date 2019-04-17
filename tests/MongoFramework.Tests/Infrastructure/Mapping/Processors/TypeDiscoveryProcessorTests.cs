using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson.Serialization;
using MongoFramework.Attributes;
using MongoFramework.Infrastructure.Mapping.Processors;
using MongoFramework.Infrastructure.Mapping.Serialization;

namespace MongoFramework.Tests.Infrastructure.Mapping.Processors
{
	[TestClass]
	public class TypeDiscoveryProcessorTests : TestBase
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
			var connection = TestConfiguration.GetConnection();
			var processor = new TypeDiscoveryProcessor();
			var classMap = new BsonClassMap<TypeDiscoveryAttributeModel>();
			classMap.AutoMap();
			BsonClassMap.RegisterClassMap(classMap);

			processor.ApplyMapping(typeof(TypeDiscoveryAttributeModel), classMap, connection);

			var serializer = BsonSerializer.LookupSerializer<TypeDiscoveryAttributeModel>();

			Assert.AreEqual(typeof(TypeDiscoverySerializer<>), serializer.GetType().GetGenericTypeDefinition());
		}

		[TestMethod]
		public void NotTypeDiscoverySerializerWhenAttributeNotDefined()
		{
			var connection = TestConfiguration.GetConnection();
			var processor = new TypeDiscoveryProcessor();
			var classMap = new BsonClassMap<NoTypeDiscoveryAttributeModel>();
			classMap.AutoMap();
			BsonClassMap.RegisterClassMap(classMap);

			processor.ApplyMapping(typeof(NoTypeDiscoveryAttributeModel), classMap, connection);

			var serializer = BsonSerializer.LookupSerializer<NoTypeDiscoveryAttributeModel>();

			Assert.AreNotEqual(typeof(TypeDiscoverySerializer<>), serializer.GetType().GetGenericTypeDefinition());
		}
	}
}
