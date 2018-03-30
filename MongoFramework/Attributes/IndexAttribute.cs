using MongoFramework.Infrastructure.Indexing;
using System;

namespace MongoFramework.Attributes
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class IndexAttribute : Attribute, IEntityIndex
	{
		public string Name { get; private set; }
		public bool IsUnique { get; set; }
		public IndexSortOrder SortOrder { get; private set; }
		public int IndexPriority { get; set; }

		public IndexAttribute(IndexSortOrder sortOrder)
		{
			SortOrder = sortOrder;
		}

		public IndexAttribute(string name, IndexSortOrder sortOrder)
		{
			Name = name;
			SortOrder = sortOrder;
		}
	}
}
