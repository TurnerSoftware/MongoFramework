using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoFramework.Attributes;

namespace MongoFramework.Tests.Mapping.Processors.RuntimeType
{
	public class RuntimeTypeMappingModel
	{
		public string Id { get; set; }

		[RuntimeTypeDiscovery]
		public CustomCollectionModel KnownCustomCollection { get; set; }
		[RuntimeTypeDiscovery]
		public IEnumerable<KnownBaseModel> KnownEnumerableInterface { get; set; }
		[RuntimeTypeDiscovery]
		public ICollection<KnownBaseModel> KnownCollectionInterface { get; set; }
		[RuntimeTypeDiscovery]
		public IList<KnownBaseModel> KnownListInterface { get; set; }
		[RuntimeTypeDiscovery]
		public List<KnownBaseModel> KnownListImplementation { get; set; }
	}
}
