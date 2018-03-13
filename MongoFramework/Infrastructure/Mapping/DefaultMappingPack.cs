﻿using System.Collections.Generic;
using MongoFramework.Infrastructure.Mapping.Processors;

namespace MongoFramework.Infrastructure.Mapping
{
	public class DefaultMappingPack : IMappingProcessorPack
	{
		public IEnumerable<IMappingProcessor> Processors { get; private set; }

		private DefaultMappingPack()
		{
			Processors = new List<IMappingProcessor>
			{
				new HierarchyProcessor(),
				new EntityIdProcessor(),
				new MappedPropertiesProcessor(),
				new NestedPropertyProcessor(),
				new ExtraElementsProcessor()
			};
		}

		public static IMappingProcessorPack Instance { get; private set; } = new DefaultMappingPack();
	}
}
