using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mapping.Processors;

namespace MongoFramework.Tests.Infrastructure.Mapping.Processors
{
	[TestClass]
	public class CollectionNameProcessorTests : MappingTestBase
	{
		[Table("CustomCollection")]
		public class CustomCollectionModel
		{
		}

		[Table("CustomCollection", Schema = "CustomSchema")]
		public class CustomCollectionAndSchemaModel
		{
		}

		public class DefaultCollectionNameModel
		{
		}

		[TestMethod]
		public void CollectionNameFromClassName()
		{
			EntityMapping.AddMappingProcessor(new CollectionNameProcessor());
			var definition = EntityMapping.RegisterType(typeof(DefaultCollectionNameModel));
			Assert.AreEqual("DefaultCollectionNameModel", definition.CollectionName);

			definition = EntityMapping.RegisterType(typeof(EntityBucket<DefaultCollectionNameModel, string>));
			Assert.AreEqual("DefaultCollectionNameModel", definition.CollectionName);
		}

		[TestMethod]
		public void CollectionNameFromAttribute()
		{
			EntityMapping.AddMappingProcessor(new CollectionNameProcessor());
			var definition = EntityMapping.RegisterType(typeof(CustomCollectionModel));
			Assert.AreEqual("CustomCollection", definition.CollectionName);

			definition = EntityMapping.RegisterType(typeof(EntityBucket<CustomCollectionModel, string>));
			Assert.AreEqual("CustomCollection", definition.CollectionName);
		}

		[TestMethod]
		public void CollectionNameAndSchemaFromAttribute()
		{
			EntityMapping.AddMappingProcessor(new CollectionNameProcessor());
			var definition = EntityMapping.RegisterType(typeof(CustomCollectionAndSchemaModel));
			Assert.AreEqual("CustomSchema.CustomCollection", definition.CollectionName);

			definition = EntityMapping.RegisterType(typeof(EntityBucket<CustomCollectionAndSchemaModel, string>));
			Assert.AreEqual("CustomSchema.CustomCollection", definition.CollectionName);
		}
	}
}
