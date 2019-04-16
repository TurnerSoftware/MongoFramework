using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoFramework.Infrastructure.Mapping.Processors;

namespace MongoFramework.Tests.Infrastructure.Mapping.Processors
{
	[TestClass]
	public class BsonKnowTypesProcessorTests : TestBase
	{
		[BsonKnownTypes(typeof(KnownTypesChildModel))]
		class KnownTypesBaseModel
		{
			public string Id { get; set; }
		}

		class KnownTypesChildModel : KnownTypesBaseModel
		{

		}

		class UnknownTypesBaseModel
		{

		}

		class UnknownTypesChildModel : UnknownTypesBaseModel
		{

		}

		[TestMethod]
		public void WithAttribute()
		{
			var connection = TestConfiguration.GetConnection();
			var processor = new BsonKnownTypesProcessor();
			var classMap = new BsonClassMap<KnownTypesBaseModel>();
			Assert.IsFalse(BsonClassMap.IsClassMapRegistered(typeof(KnownTypesChildModel)));
			processor.ApplyMapping(typeof(KnownTypesBaseModel), classMap, connection);
			Assert.IsTrue(BsonClassMap.IsClassMapRegistered(typeof(KnownTypesChildModel)));
		}

		[TestMethod]
		public void WithoutAttribute()
		{
			var connection = TestConfiguration.GetConnection();
			var processor = new BsonKnownTypesProcessor();
			var classMap = new BsonClassMap<UnknownTypesBaseModel>();
			Assert.IsFalse(BsonClassMap.IsClassMapRegistered(typeof(UnknownTypesChildModel)));
			processor.ApplyMapping(typeof(UnknownTypesBaseModel), classMap, connection);
			Assert.IsFalse(BsonClassMap.IsClassMapRegistered(typeof(UnknownTypesChildModel)));
		}
	}
}
