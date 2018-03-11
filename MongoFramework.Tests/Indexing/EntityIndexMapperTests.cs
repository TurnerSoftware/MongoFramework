using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure.Indexing;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Tests.Indexing
{
	[TestClass]
	public class EntityIndexMapperTests
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
