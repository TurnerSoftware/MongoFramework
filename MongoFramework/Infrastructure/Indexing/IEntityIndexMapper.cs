using System;
using System.Collections.Generic;

namespace MongoFramework.Infrastructure.Indexing
{
	public interface IEntityIndexMapper
	{
		Type EntityType { get; }
		string GetCollectionName();
		IEnumerable<IEntityIndexMap> GetIndexMapping();
	}
}
