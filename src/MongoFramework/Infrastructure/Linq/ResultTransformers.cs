using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace MongoFramework.Infrastructure.Linq
{
	public static class ResultTransformers
	{
		public static Expression Transform(Expression expression, Type sourceType, bool isAsync)
		{
			if (expression is MethodCallExpression methodCallExpression)
			{
				var transformName = methodCallExpression.Method.Name;
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

					var methodInfo = (transformName switch
					{
						nameof(Queryable.First) => MethodInfoCache.AsyncEnumerable.First_1,
						nameof(Queryable.FirstOrDefault) => MethodInfoCache.AsyncEnumerable.FirstOrDefault_1,

						nameof(Queryable.Single) => MethodInfoCache.AsyncEnumerable.Single_1,
						nameof(Queryable.SingleOrDefault) => MethodInfoCache.AsyncEnumerable.SingleOrDefault_1,

						nameof(Queryable.Count) => MethodInfoCache.AsyncEnumerable.SingleOrDefault_1,
						nameof(Queryable.Max) => MethodInfoCache.AsyncEnumerable.Single_1,
						nameof(Queryable.Min) => MethodInfoCache.AsyncEnumerable.Single_1,
						nameof(Queryable.Sum) => MethodInfoCache.AsyncEnumerable.Single_1,

						nameof(Queryable.Any) => MethodInfoCache.AsyncEnumerable.Any_1,

						_ => throw new InvalidOperationException($"No transform available for {transformName}")
					}).MakeGenericMethod(sourceType);

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

					var methodInfo = (transformName switch
					{
						nameof(Queryable.First) => MethodInfoCache.Enumerable.First_1,
						nameof(Queryable.FirstOrDefault) => MethodInfoCache.Enumerable.FirstOrDefault_1,

						nameof(Queryable.Single) => MethodInfoCache.Enumerable.Single_1,
						nameof(Queryable.SingleOrDefault) => MethodInfoCache.Enumerable.SingleOrDefault_1,

						nameof(Queryable.Count) => MethodInfoCache.Enumerable.SingleOrDefault_1,
						nameof(Queryable.Max) => MethodInfoCache.Enumerable.Single_1,
						nameof(Queryable.Min) => MethodInfoCache.Enumerable.Single_1,
						nameof(Queryable.Sum) => MethodInfoCache.Enumerable.Single_1,

						nameof(Queryable.Any) => MethodInfoCache.Enumerable.Any_1,

						_ => throw new InvalidOperationException($"No transform available for {transformName}")
					}).MakeGenericMethod(sourceType);

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

			throw new InvalidOperationException($"Result transformation unavailable for expression type {expression.NodeType}");
		}
	}
}