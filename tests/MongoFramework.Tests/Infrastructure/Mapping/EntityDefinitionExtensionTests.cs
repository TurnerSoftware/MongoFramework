using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Tests.Infrastructure.Mapping
{
	[TestClass]
	public class EntityDefinitionExtensionTests : TestBase
	{
		public class TraverseMappingModel
		{
			public string Id { get; set; }
			public NestedTraverseMappingModel NestedModel { get; set; }
			public NestedTraverseMappingModel RepeatedType { get; set; }
			public TraverseMappingModel RecursionType { get; set; }
		}
		public class NestedTraverseMappingModel
		{
			public string PropertyOne { get; set; }
			public int PropertyTwo { get; set; }
			public InnerNestedTraverseMappingModel InnerModel { get; set; }
		}
		public class InnerNestedTraverseMappingModel
		{
			public string InnerMostProperty { get; set; }
			public TraverseMappingModel NestedRecursionType { get; set; }
		}

		[TestMethod]
		public void TraverseMapping()
		{
			var definition = EntityMapping.RegisterType(typeof(TraverseMappingModel));
			var result = definition.TraverseProperties().ToArray();

			Assert.AreEqual(14, result.Length);
			Assert.IsTrue(result.Any(m => m.EntityType == typeof(NestedTraverseMappingModel)));
			Assert.IsTrue(result.Any(m => m.EntityType == typeof(InnerNestedTraverseMappingModel)));

			Assert.IsTrue(result.Any(m => m.FullPath == "RecursionType"));

			Assert.IsTrue(result.Any(m => m.FullPath == "NestedModel.PropertyOne"));
			Assert.IsTrue(result.Any(m => m.FullPath == "NestedModel.InnerModel"));
			Assert.IsTrue(result.Any(m => m.FullPath == "NestedModel.InnerModel.InnerMostProperty"));
			Assert.IsTrue(result.Any(m => m.FullPath == "NestedModel.InnerModel.NestedRecursionType"));


			Assert.IsTrue(result.Any(m => m.FullPath == "RepeatedType.PropertyOne"));
			Assert.IsTrue(result.Any(m => m.FullPath == "RepeatedType.InnerModel"));
			Assert.IsTrue(result.Any(m => m.FullPath == "RepeatedType.InnerModel.InnerMostProperty"));
			Assert.IsTrue(result.Any(m => m.FullPath == "RepeatedType.InnerModel.NestedRecursionType"));
		}

	}
}
