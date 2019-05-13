using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;
using MongoDB.Bson.Serialization;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class CollectionNameProcessor : IMappingProcessor
	{
		public void ApplyMapping(IEntityDefinition definition, BsonClassMap classMap)
		{
			var entityType = definition.EntityType;
			var collectionName = entityType.Name;

			var tableAttribute = entityType.GetCustomAttribute<TableAttribute>();

			if (tableAttribute == null && entityType.IsGenericType && entityType.GetGenericTypeDefinition() == typeof(EntityBucket<,>))
			{
				var groupProperty = entityType.GetProperty("Group", BindingFlags.Public | BindingFlags.Instance);
				tableAttribute = groupProperty.GetCustomAttribute<TableAttribute>();
				if (tableAttribute == null)
				{
					collectionName = groupProperty.PropertyType.Name;
				}
			}

			if (tableAttribute != null)
			{
				if (string.IsNullOrEmpty(tableAttribute.Schema))
				{
					collectionName = tableAttribute.Name;
				}
				else
				{
					collectionName = tableAttribute.Schema + "." + tableAttribute.Name;
				}
			}

			definition.CollectionName = collectionName;
		}
	}
}
