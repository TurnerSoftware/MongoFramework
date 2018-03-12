using MongoFramework.Attributes;

namespace MongoFramework.Tests.Models
{
	public class IndexNamingModel
	{
		[Index(IndexSortOrder.Ascending)]
		public string NoNameIndex { get; set; }
		[Index("MyCustomIndexName", IndexSortOrder.Ascending)]
		public string NamedIndex { get; set; }
	}
}