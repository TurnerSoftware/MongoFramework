using System;
using System.Collections.Generic;
using System.Linq;
using MongoFramework.Infrastructure.Internal;

namespace MongoFramework.Infrastructure.Mapping
{
	public static class EntityDefinitionExtensions
	{
		public static IEntityPropertyDefinition GetIdProperty(this IEntityDefinition definition)
		{
			return definition.GetAllProperties().FirstOrDefault(m => m.IsKey);
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
			var idPropertyType = definition.GetIdProperty()?.PropertyType;
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

		private sealed class TraversalState
		{
			public HashSet<Type> SeenTypes { get; set; }
			public IEnumerable<IEntityPropertyDefinition> Properties { get; set; }
		}

		public static IEnumerable<IEntityPropertyDefinition> TraverseProperties(this IEntityDefinition definition)
		{
			var stack = new Stack<TraversalState>();
			stack.Push(new TraversalState
			{
				SeenTypes = new HashSet<Type> { definition.EntityType },
				Properties = definition.GetAllProperties()
			});

			while (stack.Count > 0)
			{
				var state = stack.Pop();
				foreach (var property in state.Properties)
				{
					yield return property;

					var propertyType = property.PropertyType;
					propertyType = propertyType.GetEnumerableItemTypeOrDefault();

					if (EntityMapping.IsValidTypeToMap(propertyType) && !state.SeenTypes.Contains(propertyType))
					{
						var nestedProperties = EntityMapping.GetOrCreateDefinition(propertyType)
							.GetAllProperties()
							.Select(p => new EntityPropertyDefinition
							{
								EntityType = p.EntityType,
								IsKey = p.IsKey,
								ElementName = p.ElementName,
								FullPath = $"{property.FullPath}.{p.ElementName}",
								PropertyType = p.PropertyType,
								PropertyInfo = p.PropertyInfo
							});

						stack.Push(new TraversalState
						{
							SeenTypes = new HashSet<Type>(state.SeenTypes)
							{
								propertyType
							},
							Properties = nestedProperties
						});
					}
				}
			}
		}
	}
}
