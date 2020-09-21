using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace MongoFramework.Infrastructure.Linq
{
	public static class ResultTransformers
	{
		public static Expression Transform(Expression expression, bool isAsync)
		{
			if (expression is MethodCallExpression methodCallExpression)
			{
				var transformName = methodCallExpression.Method.Name;
				var sourceType = methodCallExpression.Method.GetGenericArguments()[0];
				if (isAsync)
				{
					var sourceParameter = Expression.Parameter(
						typeof(IAsyncEnumerable<>).MakeGenericType(sourceType),
						"source"
					);
					var cancellationTokenParameter = Expression.Parameter(
						typeof(CancellationToken),
						"cancellationToken"
					);

					var methodInfo = transformName switch
					{
						nameof(Enumerable.First) => GetMethodInfo(sourceType, () => FirstAsync<object>(default, default)),
						_ => throw new InvalidOperationException($"No transform available for {transformName}")
					};

					return Expression.Lambda(
						Expression.Call(
							null,
							methodInfo,
							sourceParameter,
							cancellationTokenParameter
						),
						sourceParameter,
						cancellationTokenParameter
					);
				}
				else
				{
					var sourceParameter = Expression.Parameter(
						typeof(IEnumerable<>).MakeGenericType(sourceType),
						"source"
					);

					var methodInfo = transformName switch
					{
						nameof(Enumerable.First) => EnumerableMethods.First_TSource_1(sourceType),
						_ => throw new InvalidOperationException($"No transform available for {transformName}")
					};

					return Expression.Lambda(
						Expression.Call(
							null,
							methodInfo,
							sourceParameter
						),
						sourceParameter
					);
				}
			}

			throw new InvalidOperationException("Unexpected expression type");
		}

		private static MethodInfo GetMethodInfo(Type sourceType, Expression<Action> expression)
		{
			var method = (expression.Body as MethodCallExpression).Method;
			return method.GetGenericMethodDefinition().MakeGenericMethod(sourceType);
		}

		private static async Task<TSource> FirstAsync<TSource>(IAsyncEnumerable<TSource> asyncEnumerable, CancellationToken cancellationToken)
		{
			await foreach (var item in asyncEnumerable)
			{
				return item;
			}

			throw new Exception("No elements in sequence");
		}
	}
}

#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed