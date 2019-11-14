using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MongoDB.Bson;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Infrastructure.Querying
{
	public static class TranslationHelper
	{
		public static MethodInfo GetMethodDefinition(Expression<Action> expression)
		{
			if (expression.Body.NodeType == ExpressionType.Call)
			{
				var methodInfo = ((MethodCallExpression)expression.Body).Method;
				return methodInfo.GetGenericMethodDefinition();
			}

			throw new InvalidOperationException("The provided expression does not call a method");
		}
	}
}
