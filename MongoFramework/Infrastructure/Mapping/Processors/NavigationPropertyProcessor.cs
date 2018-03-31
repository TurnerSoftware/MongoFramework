using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson.Serialization;
using System.Reflection;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class NavigationPropertyProcessor : IMappingProcessor
	{
		public void ApplyMapping(Type entityType, BsonClassMap classMap)
		{
			var propertyMap = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToDictionary(p => p.Name);

			foreach (var mapping in propertyMap)
			{
				var property = mapping.Value;
				var foreignKeyName = property.Name;
				var foreignKeyAttr = property.GetCustomAttribute<ForeignKeyAttribute>();

				if (foreignKeyName.EndsWith("Id") || foreignKeyAttr != null)
				{
					var navigationPropertyName = foreignKeyAttr?.Name ?? foreignKeyName.Substring(0, foreignKeyName.Length - 2);

					if (!propertyMap.ContainsKey(navigationPropertyName))
					{
						throw new MongoFrameworkMappingException($"Can't find property ${navigationPropertyName} on ${entityType.Name} for navigation property mapping.");
					}

					var navigationProperty = propertyMap[navigationPropertyName];
					classMap.UnmapMember(navigationProperty);
				}
			}
		}
	}
}
