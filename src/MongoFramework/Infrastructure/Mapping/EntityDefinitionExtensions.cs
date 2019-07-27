using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using MongoDB.Bson;
using MongoFramework.Infrastructure.Internal;

namespace MongoFramework.Infrastructure.Mapping
{
	public static class EntityDefinitionExtensions
	{
		public static IEntityProperty GetIdProperty(this IEntityDefinition definition)
		{
			return definition.GetAllProperties().Where(m => m.IsKey).FirstOrDefault();
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
			if (idPropertyType != null && idPropertyType.IsValueType)
			{
				return Activator.CreateInstance(idPropertyType);
			}
			return null;
		}

		public static IEnumerable<IEntityProperty> GetInheritedProperties(this IEntityDefinition definition)
		{
			var currentType = definition.EntityType.BaseType;
			var seenProperties = new HashSet<string>();
			while (currentType != typeof(object) && currentType != null)
			{
				var currentDefinition = EntityMapping.GetOrCreateDefinition(currentType);
				foreach (var property in currentDefinition.Properties)
				{
					if (!seenProperties.Contains(property.PropertyInfo.Name))
					{
						seenProperties.Add(property.PropertyInfo.Name);
						yield return property;
					}
				}

				currentType = currentType.BaseType;
			}
		}

		public static IEnumerable<IEntityProperty> GetAllProperties(this IEntityDefinition definition)
		{
			var result = new Dictionary<string, IEntityProperty>();
			foreach (var property in definition.Properties)
			{
				result.Add(property.PropertyInfo.Name, property);
			}
			foreach (var property in definition.GetInheritedProperties())
			{
				if (!result.ContainsKey(property.PropertyInfo.Name))
				{
					result.Add(property.PropertyInfo.Name, property);
				}
			}
			return result.Values;
		}

		public static IEntityProperty GetProperty(this IEntityDefinition definition, string name)
		{
			return definition.GetAllProperties().Where(p => p.PropertyInfo.Name == name).FirstOrDefault();
		}

		private class TraversalState
		{
			public HashSet<Type> SeenTypes { get; set; }
			public IEnumerable<IEntityProperty> Properties { get; set; }
		}
		public static IEnumerable<IEntityProperty> TraverseProperties(this IEntityDefinition definition)
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

					if (propertyType.IsClass && !state.SeenTypes.Contains(propertyType))
					{
						var nestedProperties = EntityMapping.GetOrCreateDefinition(propertyType)
							.GetAllProperties()
							.Select(p => new EntityProperty
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
