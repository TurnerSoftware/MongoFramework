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

		public class UnknownPropertyTypeModel
		{
			public string Id { get; set; }
			public object UnknownItem { get; set; }
		}

		public class UnknownPropertyTypeChildModel
		{
			public string Description { get; set; }
		}

		[TestMethod]
		public void ReadAndWriteRootEntity()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbSet<RootKnownBaseModel>(context);

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

			context.SaveChanges();

			ResetMongoDb();
			dbSet = new MongoDbSet<RootKnownBaseModel>(context);

			var dbRootEntity = dbSet.Where(e => e.Id == rootEntity.Id).FirstOrDefault();
			Assert.IsNotNull(dbRootEntity);
			Assert.IsInstanceOfType(dbRootEntity, typeof(RootKnownBaseModel));

			var dbChildEntity = dbSet.Where(e => e.Id == childEntity.Id).FirstOrDefault();
			Assert.IsNotNull(dbChildEntity);
			Assert.IsInstanceOfType(dbChildEntity, typeof(UnknownChildToRootModel));
		}

		[TestMethod]
		public void ReadAndWriteUnknownPropertyTypeEntity()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbSet<UnknownPropertyTypeModel>(context);

			var entities = new[]
			{
				new UnknownPropertyTypeModel(),
				new UnknownPropertyTypeModel
				{
					UnknownItem = new UnknownPropertyTypeChildModel
					{
						Description = "UnknownPropertyTypeChildModel"
					}
				},
				new UnknownPropertyTypeModel
				{
					UnknownItem = new Dictionary<string, int>
					{
						{ "Age", 1 }
					}
				}
			};

			dbSet.AddRange(entities);
			context.SaveChanges();

			ResetMongoDb();
			dbSet = new MongoDbSet<UnknownPropertyTypeModel>(context);

			var dbEntities = dbSet.ToArray();
			Assert.IsNull(dbEntities[0].UnknownItem);
			Assert.IsInstanceOfType(dbEntities[1].UnknownItem, typeof(UnknownPropertyTypeChildModel));
			Assert.IsInstanceOfType(dbEntities[2].UnknownItem, typeof(Dictionary<string, object>));
		}
	}
}
