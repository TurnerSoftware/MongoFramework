using MongoFramework.Infrastructure.Mapping.Processors;
using System.Collections.Generic;

namespace MongoFramework.Infrastructure.Mapping
{
	public static class DefaultProcessors
	{
		public static IEnumerable<IMappingProcessor> CreateProcessors() => new IMappingProcessor[]
		{
			new CollectionNameProcessor(),
			new HierarchyProcessor(),
			new PropertyMappingProcessor(),
			new EntityIdProcessor(),
			new NestedTypeProcessor(),
			new ExtraElementsProcessor(),
			new DecimalSerializationProcessor(),
			new TypeDiscoveryProcessor(),
			new BsonKnownTypesProcessor(),
			new IndexProcessor(),
			new MappingAdapterProcessor()
		};
	}
}
