using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using MongoFramework.Infrastructure.Mapping;
using System.ComponentModel.DataAnnotations.Schema;

namespace MongoFramework.Tests
{
	public class DefaultCollectionNameModel
	{
		public string Id { get; set; }
	}

	[Table("CustomCollection")]
	public class CustomCollectionModel
	{
		public string Id { get; set; }
	}

	[Table("CustomCollection", Schema = "CustomSchema")]
	public class CustomCollectionAndSchemaModel
	{
		public string Id { get; set; }
	}

	[TestClass]
	public class DbEntityMapperTests
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
	}
}
