using System;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MongoFramework.Infrastructure.Internal;

namespace MongoFramework.Benchmarks.Infrastructure.Internal
{
	[ShortRunJob(RuntimeMoniker.NetCoreApp30), MemoryDiagnoser]
	public class GenericMethodInvokeBenchmark
	{
		[Benchmark]
		public string DirectReflection()
		{
			var baseMethod = typeof(GenericMethodInvokeBenchmark)
					   .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
					   .Where(m => m.IsGenericMethod && m.Name == nameof(MyGenericMethod))
					   .FirstOrDefault();
			var method = baseMethod.MakeGenericMethod(typeof(int));
			return method.Invoke(null, new object[] { typeof(int), "1" }) as string;
		}

		[Benchmark]
		public string GenericHelper()
		{
			var method = GenericsHelper.GetMethodDelegate<Func<Type, string, string>>(
				typeof(GenericMethodInvokeBenchmark), 
				nameof(MyGenericMethod), 
				typeof(int)
			);

			return method(typeof(int), "1");
		}

		private static string MyGenericMethod<TType>(Type type, string output)
		{
			if (type == typeof(TType))
			{
				return output;
			}

			throw new Exception("Types don't match");
		}
	}
}
