using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using MongoDB.Bson.Serialization;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class PropertyMappingProcessor : IMappingProcessor
	{
		public void ApplyMapping(IEntityDefinition definition, BsonClassMap classMap)
		{
			var entityType = definition.EntityType;
			var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

			var definitionProperties = new List<IEntityPropertyDefinition>();

			foreach (var property in properties)
			{
				if (!property.CanRead || !property.CanWrite)
				{
					continue;
				}

				//Skip overridden properties
				var getMethod = property.GetMethod;
				if (property.GetMethod.IsVirtual && getMethod.GetBaseDefinition().DeclaringType != entityType)
				{
					continue;
				}

				//Skip indexer properties (eg. "this[int index]")
				if (property.GetIndexParameters().Length > 0)
				{
					continue;
				}

				//Skip properties with the "NotMappedAttribute"
				var notMappedAttribute = property.GetCustomAttribute<NotMappedAttribute>();
				if (notMappedAttribute != null)
				{
					continue;
				}

				var elementName = property.Name;

				//Set custom element name with the "ColumnAttribute"
				var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
				if (columnAttribute != null)
				{
					elementName = columnAttribute.Name;
				}

				definitionProperties.Add(new EntityPropertyDefinition
				{
					EntityType = definition.EntityType,
					ElementName = elementName,
					FullPath = elementName,
					PropertyType = property.PropertyType,
					PropertyInfo = property
				});
			}

			definition.Properties = definitionProperties;
		}
	}
}
