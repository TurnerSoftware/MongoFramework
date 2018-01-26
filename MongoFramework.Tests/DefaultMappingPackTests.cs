using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Tests {

	[TestClass]
	public class DefaultMappingPackTests {
		private static IMappingProcessorPack DefaultMappingPackInstance() {
			return DefaultMappingPack.Instance;
		}

		private static IEnumerable<IMappingProcessor> InitDefaultMappingPackProcessors() {
			return DefaultMappingPackInstance().Processors;
		}

		[TestMethod]
		public void TestDefaultMappingPackInstance() {
			
			var inst = DefaultMappingPack.Instance;
			var s = inst.GetType().GetProperty("Instance");
			var k = s?.PropertyType.Name;
			Assert.IsNotNull(k);
			Assert.AreEqual(k,"IMappingProcessorPack");

		}

		[TestMethod]
		public void InitDefaultMappingPackProcessorsTest1() {
			var proccessorsDefault = InitDefaultMappingPackProcessors();
			Assert.IsNotNull(proccessorsDefault);
		}

		[TestMethod]
		public void InitDefaultMappingPackProcessorsTest2() {
			var proccessorsDefault = InitDefaultMappingPackProcessors();
			Assert.IsTrue(proccessorsDefault.Count() == 5);
		}
	}
}
