using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;
using MongoDB.Bson.Serialization;

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
