
namespace MongoFramework.Infrastructure.Mapping
{
	public interface IEntityIndex
	{
		IEntityProperty Property { get; set; }
		string IndexName { get; set; }
		bool IsUnique { get; set; }
		IndexSortOrder SortOrder { get; set; }
		int IndexPriority { get; set; }
	}
}
