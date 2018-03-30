using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Linq;
using MongoFramework.Linq;
using MongoFramework.Tests.Models;
using System;
using System.Linq;

namespace MongoFramework.Tests.Linq
{
	[TestClass]
	public class LinqExtensionsTests
	{
		[TestMethod]
		public void ValidToQuery()
		{
			var collection = TestConfiguration.GetDatabase().GetCollection<LinqExtensionsModel>("LinqExtensionsModel");
			var underlyingQueryable = collection.AsQueryable();
			var queryable = new MongoFrameworkQueryable<LinqExtensionsModel, LinqExtensionsModel>(underlyingQueryable);
			var result = LinqExtensions.ToQuery(queryable);

			Assert.AreEqual("db.LinqExtensionsModel.aggregate([])", result);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException), "ArgumentException")]
		public void InvalidToQuery()
		{
			LinqExtensions.ToQuery(null);
		}
	}
}
