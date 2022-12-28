using System;
using System.Collections.Generic;
using System.Linq;

namespace MongoFramework.Infrastructure.Mapping
{
	public static class EntityDefinitionExtensions
	{
		public static IEntityPropertyDefinition GetIdProperty(this IEntityDefinition definition)
		{
			if (definition.Key is null)
			{
				return EntityMapping.GetOrCreateDefinition(definition.EntityType.BaseType).GetIdProperty();
			}

			return definition.Key?.Property;
		}

		public static string GetIdName(this IEntityDefinition definition)
		{
			return definition.GetIdProperty()?.ElementName;
		}

		public static object GetIdValue(this IEntityDefinition definition, object entity)
		{
			return definition.GetIdProperty()?.GetValue(entity);
		}

		public static object GetDefaultId(this IEntityDefinition definition)
		{
			var idPropertyType = definition.GetIdProperty()?.PropertyInfo.PropertyType;
			if (idPropertyType is { IsValueType: true })
			{
				return Activator.CreateInstance(idPropertyType);
			}
			return null;
		}

		public static IEnumerable<IEntityPropertyDefinition> GetInheritedProperties(this IEntityDefinition definition)
		{
			var currentType = definition.EntityType.BaseType;
			while (currentType != typeof(object) && currentType != null)
			{
				var currentDefinition = EntityMapping.GetOrCreateDefinition(currentType);
				foreach (var property in currentDefinition.Properties)
				{
					yield return property;
				}

				currentType = currentType.BaseType;
			}
		}

		public static IEnumerable<IEntityPropertyDefinition> GetAllProperties(this IEntityDefinition definition)
		{
			foreach (var property in definition.Properties)
			{
				yield return property;
			}

			foreach (var property in definition.GetInheritedProperties())
			{
				yield return property;
			}
		}

		public static IEntityPropertyDefinition GetProperty(this IEntityDefinition definition, string name)
		{
			foreach (var property in definition.GetAllProperties())
			{
				if (property.PropertyInfo.Name == name)
				{
					return property;
				}
			}

			return default;
		}
	}
}
