using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoFramework.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MongoFramework.Tests.Infrastructure.Serialization
{
	[TestClass]
	public class TypeDiscoveryIntegrationTests : TestBase
	{
		[RuntimeTypeDiscovery]
		public class RootKnownBaseModel
		{
			public string Id { get; set; }
			public string Description { get; set; }
		}

		public class UnknownChildToRootModel : RootKnownBaseModel
		{
			public string AdditionProperty { get; set; }
		}

		[TestMethod]
		public void ReadAndWriteRootEntity()
		{
			var connection = TestConfiguration.GetConnection();
			var dbSet = new MongoDbSet<RootKnownBaseModel>();
			dbSet.SetConnection(connection);

			var rootEntity = new RootKnownBaseModel
			{
				Description = "ReadAndWriteRootEntity-RootKnownBaseModel"
			};
			dbSet.Add(rootEntity);

			var childEntity = new UnknownChildToRootModel
			{
				Description = "ReadAndWriteRootEntity-UnknownChildToRootModel"
			};
			dbSet.Add(childEntity);

			dbSet.SaveChanges();

			ResetMongoDb();
			dbSet = new MongoDbSet<RootKnownBaseModel>();
			dbSet.SetConnection(connection);

			var dbRootEntity = dbSet.Where(e => e.Id == rootEntity.Id).FirstOrDefault();
			Assert.IsNotNull(dbRootEntity);
			Assert.AreEqual(typeof(RootKnownBaseModel), dbRootEntity.GetType());

			var dbChildEntity = dbSet.Where(e => e.Id == childEntity.Id).FirstOrDefault();
			Assert.IsNotNull(dbChildEntity);
			Assert.AreEqual(typeof(UnknownChildToRootModel), dbChildEntity.GetType());
		}
	}
}
