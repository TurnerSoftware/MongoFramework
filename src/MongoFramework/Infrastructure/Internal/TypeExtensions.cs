using System;
using System.Collections.Generic;

namespace MongoFramework.Infrastructure.Internal;

internal static class TypeExtensions
{
	private static readonly HashSet<Type> CommonGenericEnumerables = new()
	{
		typeof(IEnumerable<>),
		typeof(IList<>),
		typeof(ICollection<>),
		typeof(IReadOnlyList<>),
		typeof(IReadOnlyCollection<>)
	};

	public static Type GetEnumerableItemTypeOrDefault(this Type type)
	{
		if (type.IsArray)
		{
			return type.GetElementType();
		}
		else if (type.IsGenericType)
		{
			if (CommonGenericEnumerables.Contains(type.GetGenericTypeDefinition()))
			{
				return type.GetGenericArguments()[0];
			}
			else
			{
				//Unlike when the type is directly a known generic enumerable interface, if we start making assumptions
				//like that on the interfaces of the type, we can hit edge cases where a type implements multiple interfaces.
				foreach (var interfaceType in type.GetInterfaces())
				{
					if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
					{
						return type.GetGenericArguments()[0];
					}
				}
			}
		}

		return type;
	}
}
