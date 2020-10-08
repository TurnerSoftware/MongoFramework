
namespace MongoFramework.Infrastructure.Mapping
{
	public class EntityIndex : IEntityIndex
	{
		public IEntityProperty Property { get; set; }
		public string IndexName { get; set; }
		public bool IsUnique { get; set; }
		public IndexSortOrder SortOrder { get; set; }
		public int IndexPriority { get; set; }
		public IndexType IndexType { get; set; }
		public bool IndexTenant { get; set; }
	}
}
