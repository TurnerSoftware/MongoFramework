using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Indexing;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Serialization;

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
			EntityMapping.AddMappingProcessors(DefaultMappingProcessors.Processors);

			TypeDiscovery.ClearCache();
			EntityIndexWriter.ClearCache();

			DriverAbstractionRules.ApplyRules();
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