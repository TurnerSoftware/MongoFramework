using System.Collections.Generic;

namespace MongoFramework.Infrastructure.Indexing
{
	public interface IIndexingProcessorPack
	{
		IEnumerable<IIndexingProcessor> Processors { get; }
	}
}
