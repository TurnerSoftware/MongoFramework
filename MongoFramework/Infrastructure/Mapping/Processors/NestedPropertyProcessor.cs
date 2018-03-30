using MongoDB.Bson.Serialization;
using System;
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
				//Maps the property type for handling property nesting
				if (property.PropertyType.IsClass && property.PropertyType != entityType)
				{
					new EntityMapper(property.PropertyType);
				}
			}
		}
	}
}
