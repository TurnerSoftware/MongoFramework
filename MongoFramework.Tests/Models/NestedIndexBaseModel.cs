using MongoFramework.Attributes;

namespace MongoFramework.Tests.Models
{
	public class NestedIndexBaseModel
	{
		[Index("MyCompoundIndex", IndexSortOrder.Ascending, IndexPriority = 2)]
		public string SecondPriority { get; set; }

		public NestedIndexChildModel ChildModel { get; set; }
	}
}