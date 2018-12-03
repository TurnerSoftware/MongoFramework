using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class NestedPropertyProcessor : IMappingProcessor
	{
		public void ApplyMapping(Type entityType, BsonClassMap classMap, IMongoDbConnection connection)
		{
			var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

			foreach (var property in properties)
			{
				var propertyType = property.PropertyType;

				//Maps the property type for handling property nesting
				if (propertyType.IsClass && propertyType != entityType)
				{
					connection.GetEntityMapper(propertyType);
				}
				else if (
					propertyType.IsGenericType && propertyType.GetGenericArguments().Count() == 1 &&
					(
						propertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
						propertyType.GetInterfaces().Where(i => i.IsGenericType).Any(i => i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
					)
				)
				{
					connection.GetEntityMapper(propertyType.GetGenericArguments()[0]);
				}
			}
		}
	}
}
