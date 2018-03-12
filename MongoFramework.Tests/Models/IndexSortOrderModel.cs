using MongoFramework.Attributes;

namespace MongoFramework.Tests.Models
{
	public class IndexSortOrderModel
	{
		[Index(IndexSortOrder.Ascending)]
		public string AscendingIndex { get; set; }
		[Index(IndexSortOrder.Descending)]
		public string DescendingIndex { get; set; }
	}
}