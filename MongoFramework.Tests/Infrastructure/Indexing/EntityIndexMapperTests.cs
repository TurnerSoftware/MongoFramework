using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure.Indexing;
using MongoFramework.Infrastructure.Mapping;
using System;
using System.Linq;

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
			var entityMapper = new EntityMapper<SomeEntity>();
			var indexMapper = new EntityIndexMapper<AnotherEntity>(entityMapper);
		}
	}
}
