using System;
using System.Collections.Generic;
using System.Text;

namespace MongoFramework.Infrastructure.Mapping
{
	public interface IMappingProcessorPack
	{
		IEnumerable<IMappingProcessor> Processors { get; }
	}
}
