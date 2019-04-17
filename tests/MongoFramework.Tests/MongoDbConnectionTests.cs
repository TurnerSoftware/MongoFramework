using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MongoFramework.Tests
{
	[TestClass]
	public class MongoDbConnectionTests
	{
#if !NETCOREAPP2_0
		[TestMethod]
		public void ConnectionFromConfig()
		{
			var connection = MongoDbConnection.FromConfig("MongoFrameworkTests");
			Assert.IsNotNull(connection);
		}
		[TestMethod]
		public void InvalidConfigForConnection()
		{
			var connection = MongoDbConnection.FromConfig("ThisConfigNameDoesntExist");
			Assert.IsNull(connection);
		}
#endif

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void NullUrlThrowsException()
		{
			MongoDbConnection.FromUrl(null);
		}
	}
}
