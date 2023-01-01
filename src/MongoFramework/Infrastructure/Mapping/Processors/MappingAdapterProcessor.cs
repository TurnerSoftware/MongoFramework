using System;
using System.Reflection;
using MongoDB.Bson.Serialization;
using MongoFramework.Attributes;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class MappingAdapterProcessor : IMappingProcessor
	{
		public void ApplyMapping(EntityDefinitionBuilder definitionBuilder)
		{
			var adapterAttribute = definitionBuilder.EntityType.GetCustomAttribute<MappingAdapterAttribute>();

			if (adapterAttribute == null)
			{
				return;
			}

			var instance = (IMappingProcessor)Activator.CreateInstance(adapterAttribute.MappingAdapter);

			if (instance != null)
			{
				instance.ApplyMapping(definitionBuilder);
			}

		}
	}
}
