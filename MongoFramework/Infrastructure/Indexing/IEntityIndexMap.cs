using MongoFramework.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace MongoFramework.Infrastructure.Indexing
{
	public interface IEntityIndexMap
	{
		string ElementName { get; }
		string FullPath { get; }
		IEntityIndex Index { get; }
	}
}
