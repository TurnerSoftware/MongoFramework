using MongoFramework.Infrastructure.Mapping.Processors;
using System.Collections.Generic;

namespace MongoFramework.Infrastructure.Mapping
{
	public class DefaultMappingPack : IMappingProcessorPack
	{
		public IEnumerable<IMappingProcessor> Processors { get; }

		private DefaultMappingPack()
		{
			Processors = new List<IMappingProcessor>
			{
				new HierarchyProcessor(),
				new EntityIdProcessor(),
				new MappedPropertiesProcessor(),
				new NestedPropertyProcessor(),
				new ExtraElementsProcessor(),
				new NavigationPropertyProcessor()
			};
		}

		public static IMappingProcessorPack Instance { get; } = new DefaultMappingPack();
	}
}
