using System;
using System.Collections.Generic;
using System.Text;

namespace MongoFramework.Infrastructure.Indexing
{
	public interface IEntityIndex
	{
		string Name { get; }
		bool IsUnique { get; }
		IndexSortOrder SortOrder { get; }
	}
}
