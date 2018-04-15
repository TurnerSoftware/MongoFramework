using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;

namespace MongoFramework.Tests
{
	[TestClass]
	public abstract class DbTest
	{
		private static void ClearDatabase()
		{
			//Removing the database created for the tests
			var client = new MongoClient(TestConfiguration.ConnectionString);
			client.DropDatabase(TestConfiguration.GetDatabaseName());
		}

		[TestCleanup]
		public void Cleanup()
		{
			ClearDatabase();
		}

		[AssemblyCleanup]
		public static void AssemblyCleanup()
		{
			ClearDatabase();
		}
	}
}