using System;
using System.Collections.Generic;
using System.Linq;

namespace MongoFramework.Infrastructure.Internal
{
	internal static class TypeExtensions
	{
		public static Type GetEnumerableItemTypeOrDefault(this Type type)
		{
			if (type.IsArray)
			{
				return type.GetElementType();
			}
			else if (type.IsGenericType)
			{
				if (type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				{
					return type.GetGenericArguments()[0];
				}
				else
				{
					var compatibleInterfaces = type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
					var targetInterface = compatibleInterfaces.FirstOrDefault();
					if (targetInterface != null)
					{
						return type.GetGenericArguments()[0];
					}
				}
			}

			return type;
		}
	}
}
