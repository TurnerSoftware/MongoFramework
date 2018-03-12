using MongoFramework.Attributes;

namespace MongoFramework.Tests.Models
{
	public partial class EntityIndexWriterTests
	{
		public class IndexModel
		{
			public string Id { get; set; }
			[Index(IndexSortOrder.Ascending)]
			public string IndexedPropertyOne { get; set; }
			[Index("MyIndexedProperty", IndexSortOrder.Descending)]
			public string IndexedPropertyTwo { get; set; }
		}
	}
}