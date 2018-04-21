using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoFramework.Attributes;

namespace MongoFramework.Tests.Mapping.Serializers.TypeDiscoveryArray
{
	public class TypeDiscoveryArraySerializerModel
	{
		public string Id { get; set; }

		[RuntimeTypeDiscovery]
		public ICollection<KnownBaseModel> KnownBaseCollection { get; set; }
		public IList<IKnownBase> KnownInterfaceBaseList { get; set; }
	}
}
