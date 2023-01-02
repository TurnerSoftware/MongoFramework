using System.Collections.Generic;
using MongoFramework.Infrastructure.Mapping.Processors;

namespace MongoFramework.Infrastructure.Mapping;

public static class DefaultMappingProcessors
{
	public static readonly IReadOnlyList<IMappingProcessor> Processors = new IMappingProcessor[]
	{
		new CollectionNameProcessor(),
		new HierarchyProcessor(),
		new PropertyMappingProcessor(),
		new EntityIdProcessor(),
		new NestedTypeProcessor(),
		new ExtraElementsProcessor(),
		new BsonKnownTypesProcessor(),
		new IndexProcessor(),
		new MappingAdapterProcessor()
	};
}
