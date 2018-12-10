using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure.Mapping;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MongoFramework.Tests.Infrastructure.Mapping
{
	[TestClass]
	public class EntityMapperTests : TestBase
	{
		public class MappingLockModel
		{
			public string Id { get; set; }
		}

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
		public void CollectionNameFromClassName()
		{
			var connection = TestConfiguration.GetConnection();
			var mapper = new EntityMapper<DefaultCollectionNameModel>(connection);
			Assert.AreEqual("DefaultCollectionNameModel", mapper.GetCollectionName());
		}

		[TestMethod]
		public void CollectionNameFromAttribute()
		{
			var connection = TestConfiguration.GetConnection();
			var mapper = new EntityMapper<CustomCollectionModel>(connection);
			Assert.AreEqual("CustomCollection", mapper.GetCollectionName());
		}

		[TestMethod]
		public void CollectionNameAndSchemaFromAttribute()
		{
			var connection = TestConfiguration.GetConnection();
			var mapper = new EntityMapper<CustomCollectionAndSchemaModel>(connection);
			Assert.AreEqual("CustomSchema.CustomCollection", mapper.GetCollectionName());
		}

		[TestMethod]
		public void TraverseMapping()
		{
			var connection = TestConfiguration.GetConnection();
			var mapper = new EntityMapper<TraverseMappingModel>(connection);
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

		/// <summary>
		/// A potentially common issue for web application startup, this tests that multiple threads
		/// can map a class at the same time without concurrency issues.
		/// 
		/// Relates to: https://github.com/TurnerSoftware/MongoFramework/issues/7
		/// </summary>
		[TestMethod]
		public void MappingLocks()
		{
			var connection = TestConfiguration.GetConnection();
			AssertExtensions.DoesNotThrow<Exception>(() =>
			{
				Parallel.For(1, 10, i => { new EntityMapper<MappingLockModel>(connection); });
			});
		}
	}
}