using System;

namespace MongoFramework.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class IndexAttribute : Attribute
	{
		/// <summary>
		/// The name of the index. (Optional)
		/// </summary>
		public string Name { get; }
		/// <summary>
		/// Whether the index has a unique constraint.
		/// </summary>
		public bool IsUnique { get; set; }
		/// <summary>
		/// For standard indexes, defines the sort order of the index.
		/// </summary>
		public IndexSortOrder SortOrder { get; }
		/// <summary>
		/// THe priority of this index in relation to indexes with the same name.
		/// </summary>
		public int IndexPriority { get; set; }
		/// <summary>
		/// The type of index to be applied.
		/// </summary>
		public IndexType IndexType { get; }

		/// <summary>
		/// Applies a standard index to the property with the specified sort order.
		/// </summary>
		/// <param name="sortOrder"></param>
		public IndexAttribute(IndexSortOrder sortOrder)
		{
			SortOrder = sortOrder;
		}
		/// <summary>
		/// Applies the specified type of index to the property.
		/// </summary>
		/// <param name="indexType"></param>
		public IndexAttribute(IndexType indexType)
		{
			IndexType = indexType;
		}

		/// <summary>
		/// Applies a standard index to the property with the specified name and sort order.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="sortOrder"></param>
		public IndexAttribute(string name, IndexSortOrder sortOrder)
		{
			Name = name;
			SortOrder = sortOrder;
		}
		/// <summary>
		/// Applies the specified type of index to the property with the specified name.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="indexType"></param>
		public IndexAttribute(string name, IndexType indexType)
		{
			Name = name;
			IndexType = indexType;
		}
	}
}
