using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure.Indexing;
using System;

namespace MongoFramework.Tests.Infrastructure.Indexing
{
	[TestClass]
	public class EntityIndexMapperTests : TestBase
	{
		public class SomeEntity { }
		public class AnotherEntity { }

		[TestMethod, ExpectedException(typeof(InvalidOperationException))]
		public void EntityTypeMismatchFromEntityMapper()
		{
			var connection = TestConfiguration.GetConnection();
			var indexMapper = new EntityIndexMapper<AnotherEntity>(connection.GetEntityMapper(typeof(SomeEntity)));
		}
	}
}
