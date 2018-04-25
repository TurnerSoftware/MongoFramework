using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoFramework.Attributes;

namespace MongoFramework.Tests.Mapping.Serialization.TypeDiscovery
{
	public class UnknownChildToRootModel : RootKnownBaseModel
	{
		public string AdditionProperty { get; set; }
	}
}
