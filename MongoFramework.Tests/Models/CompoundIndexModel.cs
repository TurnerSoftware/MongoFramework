using MongoFramework.Attributes;

namespace MongoFramework.Tests.Models
{
	public class CompoundIndexModel
	{
		[Index("MyCompoundIndex", IndexSortOrder.Ascending, IndexPriority = 1)]
		public string FirstPriority { get; set; }

		[Index("MyCompoundIndex", IndexSortOrder.Ascending, IndexPriority = 3)]
		public string ThirdPriority { get; set; }

		[Index("MyCompoundIndex", IndexSortOrder.Ascending, IndexPriority = 2)]
		public string SecondPriority { get; set; }
	}
}