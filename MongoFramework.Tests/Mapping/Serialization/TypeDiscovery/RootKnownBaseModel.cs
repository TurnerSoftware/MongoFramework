using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoFramework.Attributes;

namespace MongoFramework.Tests.Mapping.Serialization.TypeDiscovery
{
	[RuntimeTypeDiscovery]
	public class RootKnownBaseModel
	{
		public string Id { get; set; }
		public string Description { get; set; }
	}
}
