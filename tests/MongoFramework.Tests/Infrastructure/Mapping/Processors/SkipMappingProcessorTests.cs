using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mapping.Processors;

namespace MongoFramework.Tests.Infrastructure.Mapping.Processors
{
	[TestClass]
	public class SkipMappingProcessorTests : MappingTestBase
	{
		[NotMapped]
		public class SkippedMappingModel
		{
		}

		public class DefaultModel
		{
		}

		[TestMethod]
		public void ModelSkippedWithAttribute()
		{
			EntityMapping.AddMappingProcessor(new SkipMappingProcessor());

			var exception = Assert.ThrowsException<ArgumentException>(() => EntityMapping.RegisterType(typeof(SkippedMappingModel)));
			Assert.IsTrue(exception.Message.Contains("was skipped"));
		}

		[TestMethod]
		public void ModelNotSkipped()
		{
			EntityMapping.AddMappingProcessor(new SkipMappingProcessor());

			var definition = EntityMapping.RegisterType(typeof(DefaultModel));
			Assert.IsNotNull(definition);
		}
	}
}
