﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace MongoFramework.Infrastructure.Mapping
{
	public static class EntityDefinitionExtensions
	{
		/// <summary>
		/// Finds the nearest <see cref="KeyDefinition"/> from <paramref name="definition"/>, recursively searching the base <see cref="EntityDefinition"/> if one exists.
		/// </summary>
		/// <param name="definition">The <see cref="EntityDefinition"/> to start the search from.</param>
		/// <returns>The key definition; otherwise <see langword="null"/> if one can not be found.</returns>
		public static KeyDefinition FindNearestKey(this EntityDefinition definition)
		{
			if (definition.Key is null)
			{
				return EntityMapping.GetOrCreateDefinition(definition.EntityType.BaseType).FindNearestKey();
			}

			return definition.Key;
		}

		public static PropertyDefinition GetIdProperty(this EntityDefinition definition)
		{
			return definition.FindNearestKey()?.Property;
		}

		public static string GetIdName(this EntityDefinition definition)
		{
			return definition.GetIdProperty()?.ElementName;
		}

		public static object GetIdValue(this EntityDefinition definition, object entity)
		{
			return definition.GetIdProperty()?.GetValue(entity);
		}

		public static object GetDefaultId(this EntityDefinition definition)
		{
			var idPropertyType = definition.GetIdProperty()?.PropertyInfo.PropertyType;
			if (idPropertyType is { IsValueType: true })
			{
				return Activator.CreateInstance(idPropertyType);
			}
			return null;
		}

		public static IEnumerable<PropertyDefinition> GetInheritedProperties(this EntityDefinition definition)
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

		public static IEnumerable<PropertyDefinition> GetAllProperties(this EntityDefinition definition)
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

		public static PropertyDefinition GetProperty(this EntityDefinition definition, string name)
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
