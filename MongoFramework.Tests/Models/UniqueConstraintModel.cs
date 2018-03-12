using MongoFramework.Attributes;

namespace MongoFramework.Tests.Models
{
	public class UniqueConstraintModel
	{
		[Index("UniqueIndex", IndexSortOrder.Ascending, IsUnique = true)]
		public string UniqueIndex { get; set; }

		[Index("NonUniqueIndex", IndexSortOrder.Ascending, IsUnique = false)]
		public string NotUniqueIndex { get; set; }
	}
}