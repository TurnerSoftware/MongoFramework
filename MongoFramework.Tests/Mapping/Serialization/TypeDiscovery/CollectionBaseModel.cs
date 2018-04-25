using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Tests.Mapping.Serialization.TypeDiscovery
{
	public class CollectionBaseModel
	{
		public IList<KnownBaseModel> KnownList { get; set; }
	}
}
