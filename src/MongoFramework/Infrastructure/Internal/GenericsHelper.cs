using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MongoFramework.Infrastructure.Internal
{
	internal static class GenericsHelper
	{
		private static readonly ConcurrentDictionary<(Type DeclaringType, string MethodName, Type GenericType), Delegate> MethodDelegateLookup = new ConcurrentDictionary<(Type, string, Type), Delegate>();

		/// <summary>
		/// Creates a delegate for a method which includes a generic argument.
		/// If a corresponding delegate already exists, it is returned.
		/// </summary>
		/// <typeparam name="TDelegate">The target delegate type.</typeparam>
		/// <param name="declaringType">The type where the static target method resides.</param>
		/// <param name="methodName">The name of the target method.</param>
		/// <param name="genericArgument">The generic type for the target method.</param>
		/// <returns></returns>
		public static TDelegate GetMethodDelegate<TDelegate>(Type declaringType, string methodName, Type genericArgument) where TDelegate : Delegate
		{
			return (TDelegate)MethodDelegateLookup.GetOrAdd((declaringType, methodName, genericArgument), o =>
			{
				var baseMethod = o.DeclaringType
					.GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
					.Where(m => m.IsGenericMethod && m.Name == methodName)
					.FirstOrDefault();
				return baseMethod.MakeGenericMethod(genericArgument).CreateDelegate(typeof(TDelegate));
			});
		}
	}
}
