using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class NestedPropertyProcessor : IMappingProcessor
	{
		public void ApplyMapping(Type entityType, BsonClassMap classMap)
		{
			var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

			foreach (var property in properties)
			{
				var propertyType = property.PropertyType;

				//Maps the property type for handling property nesting
				if (propertyType.IsClass && propertyType != entityType)
				{
					new EntityMapper(property.PropertyType);
				}
				else if (
					propertyType.IsGenericType && propertyType.GetGenericArguments().Count() == 1 &&
					(
						propertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
						propertyType.GetInterfaces().Where(i => i.IsGenericType).Any(i => i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
					)
				)
				{
					new EntityMapper(propertyType.GetGenericArguments()[0]);
				}
			}
		}
	}
}
