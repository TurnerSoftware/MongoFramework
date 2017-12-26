using System;
using System.Collections.Generic;
using System.Text;

namespace MongoFramework.Infrastructure.Indexing
{
	public interface IIndexingProcessorPack
	{
		IEnumerable<IIndexingProcessor> Processors { get; }
	}
}
