using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Tests.Infrastructure.Mapping;

[TestClass]
public class PropertyTraversalExtensionTests : TestBase
{
	public class TraverseMappingModel
	{
		public string Id { get; set; }
		public NestedTraverseMappingModel NestedModel { get; set; }
		public NestedTraverseMappingModel RepeatedType { get; set; }
		public TraverseMappingModel RecursionType { get; set; }

		public NestedTraverseMappingModel[] ArrayModel { get; set; }
		public IEnumerable<NestedTraverseMappingModel> EnumerableModel { get; set; }
		public List<NestedTraverseMappingModel> ListModel { get; set; }

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
	public void TraverseProperties()
	{
		var definition = EntityMapping.RegisterType(typeof(TraverseMappingModel));
		var result = definition.TraverseProperties().ToArray();

		Assert.AreEqual(32, result.Length);

		Assert.IsTrue(result.Any(m => m.GetPath() == "RecursionType" && m.Depth == 0));

		Assert.IsTrue(result.Any(m => m.GetPath() == "NestedModel.PropertyOne" && m.Depth == 1));
		Assert.IsTrue(result.Any(m => m.GetPath() == "NestedModel.InnerModel" && m.Depth == 1));
		Assert.IsTrue(result.Any(m => m.GetPath() == "NestedModel.InnerModel.InnerMostProperty" && m.Depth == 2));
		Assert.IsTrue(result.Any(m => m.GetPath() == "NestedModel.InnerModel.NestedRecursionType" && m.Depth == 2));

		Assert.IsTrue(result.Any(m => m.GetPath() == "RepeatedType.PropertyOne" && m.Depth == 1));
		Assert.IsTrue(result.Any(m => m.GetPath() == "RepeatedType.InnerModel" && m.Depth == 1));
		Assert.IsTrue(result.Any(m => m.GetPath() == "RepeatedType.InnerModel.InnerMostProperty" && m.Depth == 2));
		Assert.IsTrue(result.Any(m => m.GetPath() == "RepeatedType.InnerModel.NestedRecursionType" && m.Depth == 2));

		Assert.IsTrue(result.Any(m => m.GetPath() == "ArrayModel" && m.Depth == 0));
		Assert.IsTrue(result.Any(m => m.GetPath() == "ArrayModel.InnerModel" && m.Depth == 1));
		Assert.IsTrue(result.Any(m => m.GetPath() == "ArrayModel.InnerModel.InnerMostProperty" && m.Depth == 2));
		Assert.IsTrue(result.Any(m => m.GetPath() == "ArrayModel.InnerModel.NestedRecursionType" && m.Depth == 2));

		Assert.IsTrue(result.Any(m => m.GetPath() == "EnumerableModel" && m.Depth == 0));
		Assert.IsTrue(result.Any(m => m.GetPath() == "EnumerableModel.InnerModel" && m.Depth == 1));
		Assert.IsTrue(result.Any(m => m.GetPath() == "EnumerableModel.InnerModel.InnerMostProperty" && m.Depth == 2));
		Assert.IsTrue(result.Any(m => m.GetPath() == "EnumerableModel.InnerModel.NestedRecursionType" && m.Depth == 2));

		Assert.IsTrue(result.Any(m => m.GetPath() == "ListModel" && m.Depth == 0));
		Assert.IsTrue(result.Any(m => m.GetPath() == "ListModel.InnerModel" && m.Depth == 1));
		Assert.IsTrue(result.Any(m => m.GetPath() == "ListModel.InnerModel.InnerMostProperty" && m.Depth == 2));
		Assert.IsTrue(result.Any(m => m.GetPath() == "ListModel.InnerModel.NestedRecursionType" && m.Depth == 2));
	}
}
