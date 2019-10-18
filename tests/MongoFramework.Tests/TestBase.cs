using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Serialization;
using System;

namespace MongoFramework.Tests
{
	[TestClass]
	public abstract class TestBase
	{
		protected static void ResetMongoDb()
		{
			MongoDbDriverHelper.ResetDriver();
			EntityMapping.RemoveAllDefinitions();

			EntityMapping.RemoveAllMappingProcessors();
			EntityMapping.AddMappingProcessors(DefaultMappingPack.Instance.Processors);

			TypeDiscovery.ClearCache();
		}

		protected static void ClearDatabase()
		{
			//Removing the database created for the tests
			var client = new MongoClient(TestConfiguration.ConnectionString);
			client.DropDatabase(TestConfiguration.GetDatabaseName());
		}

		[TestInitialize]
		public void Initialise()
		{
			ResetMongoDb();
			ClearDatabase();
		}

		[AssemblyCleanup]
		public static void AssemblyCleanup()
		{
			ClearDatabase();
		}
	}
}