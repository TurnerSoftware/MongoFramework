using MongoFramework.Infrastructure.Indexing.Processors;
using System;
using System.Collections.Generic;
using System.Text;

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

		public static IIndexingProcessorPack Instance { get; private set; } = new DefaultIndexingPack();
	}
}
