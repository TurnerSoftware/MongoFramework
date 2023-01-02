using System;
using System.Reflection;
using MongoFramework.Attributes;

namespace MongoFramework.Infrastructure.Mapping.Processors;

public class MappingAdapterProcessor : IMappingProcessor
{
	public void ApplyMapping(EntityDefinitionBuilder definitionBuilder)
	{
		var adapterAttribute = definitionBuilder.EntityType.GetCustomAttribute<MappingAdapterAttribute>();
		if (adapterAttribute == null)
		{
			return;
		}

		var adapterType = adapterAttribute.MappingAdapter;
		if (!typeof(IMappingProcessor).IsAssignableFrom(adapterType))
		{
			throw new InvalidOperationException($"Mapping adapter \"{adapterType}\" doesn't implement IMappingProcessor");
		}

		var instance = (IMappingProcessor)Activator.CreateInstance(adapterType);
		instance?.ApplyMapping(definitionBuilder);
	}
}
