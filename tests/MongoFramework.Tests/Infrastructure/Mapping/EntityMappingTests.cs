using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure.Mapping;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MongoFramework.Tests.Infrastructure.Mapping
{
	[TestClass]
	public class EntityMappingTests : TestBase
	{
		public class MappingLockModel
		{
			public string Id { get; set; }
		}

		/// <summary>
		/// A potentially common issue for web application startup, this tests that multiple threads
		/// can map a class at the same time without concurrency issues.
		/// 
		/// Relates to: https://github.com/TurnerSoftware/MongoFramework/issues/7
		/// </summary>
		[TestMethod]
		public void MappingLocks()
		{
			var connection = TestConfiguration.GetConnection();
			AssertExtensions.DoesNotThrow<Exception>(() =>
			{
				Parallel.For(1, 10, i => { EntityMapping.GetOrCreateDefinition(typeof(MappingLockModel)); });
			});
		}
	}
}