using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;
using MongoDB.Bson.Serialization;
using MongoFramework.Attributes;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class MappingAdapterProcessor : IMappingProcessor
	{
		public void ApplyMapping(IEntityDefinition definition, BsonClassMap classMap)
		{
			var adapterAttribute = definition.EntityType.GetCustomAttribute<MappingAdapterAttribute>();

			if (adapterAttribute == null)				
			{
				return;
			}

			var instance = (IMappingProcessor)Activator.CreateInstance(adapterAttribute.MappingAdapter);

			if (instance != null)
			{
				instance.ApplyMapping(definition, classMap);
			}

		}
	}
}
