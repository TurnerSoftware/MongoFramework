using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Tests.Infrastructure.Mapping
{
	[TestClass]
	public abstract class MappingTestBase : TestBase
	{
		[TestInitialize]
		public void MappingProcessorReset()
		{
			EntityMapping.RemoveAllMappingProcessors();
		}
	}
}
