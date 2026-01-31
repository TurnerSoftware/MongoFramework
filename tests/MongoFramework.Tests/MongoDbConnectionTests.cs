using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MongoFramework.Tests
{
	[TestClass]
	public class MongoDbConnectionTests
	{
		[TestMethod]
		public void ConnectionFromConnectionString()
		{
			var connection = MongoDbConnection.FromConnectionString("mongodb://localhost:27017/MongoFrameworkTests");
			Assert.IsNotNull(connection);
		}

		[TestMethod]
		public void NullUrlThrowsException()
		{
			Assert.ThrowsException<ArgumentNullException>(() => MongoDbConnection.FromUrl(null));
		}
	}
}
