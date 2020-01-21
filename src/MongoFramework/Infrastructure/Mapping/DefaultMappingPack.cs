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
				new CollectionNameProcessor(),
				new HierarchyProcessor(),
				new PropertyMappingProcessor(),
				new EntityIdProcessor(),
				new NestedTypeProcessor(),
				new ExtraElementsProcessor(),
				new TypeDiscoveryProcessor(),
				new BsonKnownTypesProcessor(),
				new IndexProcessor(),
				new EntityRelationshipProcessor()
			};
		}
		public static IMappingProcessorPack Instance { get; } = new DefaultMappingPack();
	}
}
