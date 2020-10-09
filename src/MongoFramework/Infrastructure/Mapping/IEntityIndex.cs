
namespace MongoFramework.Infrastructure.Mapping
{
	public interface IEntityIndex
	{
		IEntityProperty Property { get; }
		string IndexName { get; }
		bool IsUnique { get; }
		IndexSortOrder SortOrder { get; }
		int IndexPriority { get; }
		IndexType IndexType { get; }
		bool IsTenantExclusive { get; set; }
	}
}
