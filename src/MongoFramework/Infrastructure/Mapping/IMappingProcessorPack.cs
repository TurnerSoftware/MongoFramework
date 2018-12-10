using System.Collections.Generic;

namespace MongoFramework.Infrastructure.Mapping
{
	public interface IMappingProcessorPack
	{
		IEnumerable<IMappingProcessor> Processors { get; }
	}
}
