using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class PropertyMappingProcessor : IMappingProcessor
	{
		public void ApplyMapping(IEntityDefinition definition, BsonClassMap classMap)
		{
			var entityType = definition.EntityType;
			var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

			var definitionProperties = new List<IEntityProperty>();

			foreach (var property in properties)
			{
				if (!property.CanRead)
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

				//Do the mapping
				var memberMap = classMap.MapMember(property);
				
				//Set custom element name with the "ColumnAttribute"
				var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
				if (columnAttribute != null)
				{
					var mappedName = columnAttribute.Name;
					memberMap.SetElementName(mappedName);
				}

				definitionProperties.Add(new EntityProperty
				{
					EntityType = definition.EntityType,
					ElementName = memberMap.ElementName,
					FullPath = memberMap.ElementName,
					PropertyType = property.PropertyType,
					PropertyInfo = property
				});
			}

			definition.Properties = definitionProperties;
		}
	}
}
