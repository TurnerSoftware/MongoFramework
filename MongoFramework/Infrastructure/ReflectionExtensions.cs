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

				//Remove details of array item - we don't know instance-specific information so it doesn't help
				if (currentName.Contains('[') && currentName.Contains(']'))
				{
					currentName = currentName.Substring(0, currentName.IndexOf('['));
				}

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
				if (namePieces.Any() && propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				{
					propertyType = propertyType.GetGenericArguments()[0];
				}

				currentType = propertyType;
			}


			return null;
		}
	}
}
