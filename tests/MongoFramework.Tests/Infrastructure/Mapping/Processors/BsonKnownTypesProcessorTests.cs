using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mapping.Processors;

namespace MongoFramework.Tests.Infrastructure.Mapping.Processors
{
	[TestClass]
	public class BsonKnownTypesProcessorTests : MappingTestBase
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
			EntityMapping.AddMappingProcessor(new BsonKnownTypesProcessor());
			Assert.IsFalse(BsonClassMap.IsClassMapRegistered(typeof(KnownTypesChildModel)));
			EntityMapping.RegisterType(typeof(KnownTypesBaseModel));
			Assert.IsTrue(BsonClassMap.IsClassMapRegistered(typeof(KnownTypesChildModel)));
		}

		[TestMethod]
		public void WithoutAttribute()
		{
			EntityMapping.AddMappingProcessor(new BsonKnownTypesProcessor());
			Assert.IsFalse(BsonClassMap.IsClassMapRegistered(typeof(UnknownTypesChildModel)));
			EntityMapping.RegisterType(typeof(UnknownTypesBaseModel));
			Assert.IsFalse(BsonClassMap.IsClassMapRegistered(typeof(UnknownTypesChildModel)));
		}
	}
}
