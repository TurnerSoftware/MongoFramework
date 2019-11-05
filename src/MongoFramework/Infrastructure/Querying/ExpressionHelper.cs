using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MongoDB.Bson;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Infrastructure.Querying
{
	public static class ExpressionHelper
	{
		public static Expression<Func<T1>> Stub<T1>()
		{
			return null;
		}
		public static Expression<Func<T1, T2>> Stub<T1, T2>()
		{
			return null;
		}
		public static Expression<Func<T1, T2, T3>> Stub<T1, T2, T3>()
		{
			return null;
		}
		public static Expression<Func<T1, T2, T3, T4>> Stub<T1, T2, T3, T4>()
		{
			return null;
		}

		public static MethodInfo GetGenericMethodInfo(Expression<Action> expression)
		{
			if (expression.Body.NodeType == ExpressionType.Call)
			{
				var methodInfo = ((MethodCallExpression)expression.Body).Method;
				return methodInfo.GetGenericMethodDefinition();
			}

			throw new InvalidOperationException("The provided expression does not call a method");
		}

		public static BsonDocument Where(LambdaExpression expression)
		{
			if (expression.ReturnType != typeof(bool))
			{
				throw new ArgumentException("Expression must return a boolean");
			}

			var incomingType = expression.Parameters[0].Type;

			if (EntityMapping.IsRegistered(incomingType))
			{
				var definition = EntityMapping.GetOrCreateDefinition(incomingType);

				
			}
			return null;
		}
	}
}
