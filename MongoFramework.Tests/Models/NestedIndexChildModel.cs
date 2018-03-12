using MongoFramework.Attributes;

namespace MongoFramework.Tests.Models
{
	public class NestedIndexChildModel
	{
		[Index("MyCompoundIndex", IndexSortOrder.Ascending, IndexPriority = 1)]
		public string FirstPriority { get; set; }
	}
}