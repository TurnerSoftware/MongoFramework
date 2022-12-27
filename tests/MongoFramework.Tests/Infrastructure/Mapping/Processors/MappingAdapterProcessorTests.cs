using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson.Serialization;
using MongoFramework.Attributes;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mapping.Processors;

namespace MongoFramework.Tests.Infrastructure.Mapping.Processors
{
	[TestClass]
	public class MappingAdapterProcessorTests : MappingTestBase
	{

		public class AdapterTestModelMappingAdapter : IMappingProcessor
		{
			public void ApplyMapping(IEntityDefinition definition, BsonClassMap classMap)
			{
				definition.CollectionName = "Custom";

				var definitionIndexes = definition.Indexes.ToList();

				definitionIndexes.Add(new EntityIndexDefinition
				{
					Property = definition.GetProperty("UserName"),
					IsUnique = true,
					SortOrder = IndexSortOrder.Ascending
				});

				definition.Indexes = definitionIndexes;
			}
		}

		[Table("TestModels")]
		[MappingAdapter(typeof(AdapterTestModelMappingAdapter))]
		public class AdapterTestModel
		{
			public Guid MyCustomId { get; set; }
			public string UserName { get; set; }

		}

		public class AdapterTestModelMappingAdapterNoInterface
		{
			// no interface
		}

		[MappingAdapter(typeof(AdapterTestModelMappingAdapterNoInterface))]
		public class AdapterTestModelNoInterface
		{
			// broken adapter
		}

		public class AdapterTestModelMappingAdapterConstructor : IMappingProcessor
		{
			public AdapterTestModelMappingAdapterConstructor(string test)
			{

			}

			public void ApplyMapping(IEntityDefinition definition, BsonClassMap classMap)
			{
				throw new NotImplementedException();
			}
		}

		[MappingAdapter(typeof(AdapterTestModelMappingAdapterConstructor))]
		public class AdapterTestModelConstructor
		{
			// broken adapter
		}

		[TestMethod]
		public void AdapterRequiresIMappingProcessor()
		{
			EntityMapping.AddMappingProcessor(new CollectionNameProcessor());
			EntityMapping.AddMappingProcessor(new PropertyMappingProcessor());
			EntityMapping.AddMappingProcessor(new EntityIdProcessor());
			EntityMapping.AddMappingProcessor(new MappingAdapterProcessor());
			Assert.ThrowsException<ArgumentException>(() => EntityMapping.RegisterType(typeof(AdapterTestModelNoInterface)));
		}

		[TestMethod]
		public void AdapterRequiresParameterlessConstructor()
		{
			EntityMapping.AddMappingProcessor(new CollectionNameProcessor());
			EntityMapping.AddMappingProcessor(new PropertyMappingProcessor());
			EntityMapping.AddMappingProcessor(new EntityIdProcessor());
			EntityMapping.AddMappingProcessor(new MappingAdapterProcessor());
			Assert.ThrowsException<MissingMethodException>(() => EntityMapping.RegisterType(typeof(AdapterTestModelConstructor)));
		}

		[TestMethod]
		public void AdapterOverridesAttributes()
		{
			EntityMapping.AddMappingProcessor(new CollectionNameProcessor());
			EntityMapping.AddMappingProcessor(new PropertyMappingProcessor());
			EntityMapping.AddMappingProcessor(new EntityIdProcessor());
			EntityMapping.AddMappingProcessor(new MappingAdapterProcessor());
			var definition = EntityMapping.RegisterType(typeof(AdapterTestModel));

			Assert.AreEqual("Custom", definition.CollectionName);
			Assert.AreEqual(1, definition.Indexes.Count());



		}

	}
}