using MongoDB.Bson.Serialization;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class MappedPropertiesProcessor : IMappingProcessor
	{
		public void ApplyMapping(Type entityType, BsonClassMap classMap)
		{
			var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

			foreach (var property in properties)
			{
				//Unmap fields with the "NotMappedAttribute"
				var notMappedAttribute = property.GetCustomAttribute<NotMappedAttribute>();
				if (notMappedAttribute != null)
				{
					classMap.UnmapProperty(property.Name);
					continue;
				}

				//Remap fields with the "ColumnAttribute"
				var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
				if (columnAttribute != null)
				{
					var mappedName = columnAttribute.Name;
					var memberMap = classMap.GetMemberMap(property.Name);
					memberMap?.SetElementName(mappedName);
				}
			}
		}
	}
}
