using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;

namespace MongoFramework.Tests
{
	[TestClass]
	public class TestSetup
	{
		[AssemblyCleanup]
		public static void AssemblyCleanup()
		{
			//Removing the database created for the tests
			var client = new MongoClient(TestConfiguration.ConnectionString);
			client.DropDatabase(TestConfiguration.GetDatabaseName());
		}
	}
}