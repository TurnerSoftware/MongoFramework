using MongoFramework.Infrastructure.Indexing.Processors;
using System.Collections.Generic;

namespace MongoFramework.Infrastructure.Indexing
{
	public class DefaultIndexingPack : IIndexingProcessorPack
	{
		public IEnumerable<IIndexingProcessor> Processors { get; private set; }

		private DefaultIndexingPack()
		{
			Processors = new List<IIndexingProcessor>
			{
				new BasicIndexProcessor()
			};
		}

		public static IIndexingProcessorPack Instance { get; } = new DefaultIndexingPack();
	}
}
