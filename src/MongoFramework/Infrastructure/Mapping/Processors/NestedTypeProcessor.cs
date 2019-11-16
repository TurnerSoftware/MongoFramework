using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class NestedTypeProcessor : IMappingProcessor
	{
		public void ApplyMapping(IEntityDefinition definition, BsonClassMap classMap)
		{
			var entityType = definition.EntityType;
			var properties = definition.Properties;

			foreach (var property in properties)
			{
				var propertyType = property.PropertyType;
				propertyType = propertyType.GetEnumerableItemTypeOrDefault();

				//Maps the property type for handling property nesting
				if (propertyType != entityType && EntityMapping.IsValidTypeToMap(propertyType))
				{
					if (!EntityMapping.IsRegistered(propertyType))
					{
						EntityMapping.RegisterType(propertyType);
					}
				}
			}
		}
	}
}
