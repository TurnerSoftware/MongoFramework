using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using MongoFramework.Infrastructure.Mapping;
using System.ComponentModel.DataAnnotations.Schema;

namespace MongoFramework.Tests
{
	public class DefaultCollectionNameModel
	{
	}

	[Table("CustomCollection")]
	public class CustomCollectionModel
	{
	}

	[Table("CustomCollection", Schema = "CustomSchema")]
	public class CustomCollectionAndSchemaModel
	{
	}

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

	[TestClass]
	public class EntityMapperTests
	{
		[TestMethod]
		public void CollectionNameFromClassName()
		{
			var mapper = new EntityMapper<DefaultCollectionNameModel>();
			Assert.AreEqual("DefaultCollectionNameModel", mapper.GetCollectionName());
		}

		[TestMethod]
		public void CollectionNameFromAttribute()
		{
			var mapper = new EntityMapper<CustomCollectionModel>();
			Assert.AreEqual("CustomCollection", mapper.GetCollectionName());
		}

		[TestMethod]
		public void CollectionNameAndSchemaFromAttribute()
		{
			var mapper = new EntityMapper<CustomCollectionAndSchemaModel>();
			Assert.AreEqual("CustomSchema.CustomCollection", mapper.GetCollectionName());
		}

		[TestMethod]
		public void TraverseMapping()
		{
			var mapper = new EntityMapper<TraverseMappingModel>();
			var result = mapper.TraverseMapping().ToArray();

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
