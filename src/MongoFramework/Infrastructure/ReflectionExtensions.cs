using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MongoFramework.Infrastructure
{
	internal static class ReflectionExtensions
	{
		public static PropertyInfo GetNestedProperty(this Type parentType, string name)
		{
			if (name == null)
			{
				return null;
			}

			var namePieces = new Stack<string>(name.Split('.'));
			var currentType = parentType;

			while (namePieces.Any())
			{
				var currentName = namePieces.Pop();

				var property = currentType.GetProperty(currentName);
				if (property == null)
				{
					return null;
				}
				else if (!namePieces.Any())
				{
					return property;
				}

				var propertyType = property.PropertyType;
				if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				{
					propertyType = propertyType.GetGenericArguments()[0];
				}

				//TODO: Add support to check all interfaces to find generic type definition
				//		See: https://stackoverflow.com/a/1121864/1676444

				currentType = propertyType;
			}

			return null;
		}
	}
}
