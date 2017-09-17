using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Bson
{
	internal static class ReflectionExtensions
	{
		public static Type GetNestedPropertyType(this Type parentType, string name)
		{
			if (name == null)
			{
				return null;
			}
			
			var namesPieces = new Stack<string>(name.Split('.'));
			return parentType.GetNestedPropertyType(namesPieces);
		}

		private static Type GetNestedPropertyType(this Type parentType, Stack<string> namePieces)
		{
			if (!namePieces.Any())
			{
				return parentType;
			}
			
			var currentName = namePieces.Pop();

			//Remove details of array item - we don't know instance-specific information so it doesn't help
			if (currentName.Contains('[') && currentName.Contains(']'))
			{
				currentName = currentName.Substring(0, currentName.IndexOf('['));
			}

			var property = parentType.GetProperty(currentName);
			if (property == null)
			{
				return null;
			}

			var propertyType = property.PropertyType;
			if (namePieces.Any() && propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
			{
				propertyType = propertyType.GetGenericArguments()[0];
			}

			return propertyType.GetNestedPropertyType(namePieces);
		}
	}
}
