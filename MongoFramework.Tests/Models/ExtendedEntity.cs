using System.Collections.Generic;

namespace MongoFramework.Tests.Models
{
	class ExtendedEntity : CommonEntity
	{
		public bool IsDisabled { get; set; }
		public IDictionary<string, string> CustomData { get; set; }
		public IEnumerable<string> Tags { get; set; }
		public SingleRelation Single { get; set; }
		public IList<ListRelationItem> Multiple { get; set; }
	}
}