using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MongoDB.Bson;
using MongoFramework.Infrastructure.Querying.CallConverters;

namespace MongoFramework.Infrastructure.Querying
{
	public static class QueryMapping
	{
		public static ConcurrentDictionary<MethodInfo, ICallConverter> CallConverters { get; } = new ConcurrentDictionary<MethodInfo, ICallConverter>();

		static QueryMapping()
		{
			AddConverter(WhereConverter.Instance, () => Queryable.Where(default, ExpressionHelper.Stub<object, bool>()));
		}

		public static void AddConverter(ICallConverter callConverter, params Expression<Action>[] methodExpressions)
		{
			foreach (var expression in methodExpressions)
			{
				var methodInfo = ExpressionHelper.GetGenericMethodInfo(expression);
				CallConverters.TryAdd(methodInfo, callConverter);
			}
		}

		public static IEnumerable<BsonDocument> FromExpression(Expression expression)
		{
			var currentExpression = expression;
			var stages = new Stack<BsonDocument>();

			while (currentExpression is MethodCallExpression callExpression)
			{
				var methodDefinition = callExpression.Method;
				if (methodDefinition.IsGenericMethod)
				{
					methodDefinition = methodDefinition.GetGenericMethodDefinition();
				}

				if (CallConverters.TryGetValue(methodDefinition, out var converter))
				{
					var queryStage = converter.Convert(callExpression);
					stages.Push(queryStage);
					currentExpression = callExpression.Arguments[0];
				}
				else
				{
					throw new InvalidOperationException($"No converter has been configured for {callExpression.Method}");
				}
			}

			return stages;
		}
	}
}
