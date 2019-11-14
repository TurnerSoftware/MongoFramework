using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using MongoDB.Bson;

namespace MongoFramework.Infrastructure.Querying
{
	public static class StageBuilder
	{
		public static IEnumerable<BsonDocument> BuildFromExpression(Expression expression)
		{
			var currentExpression = expression;
			var stages = new Stack<BsonDocument>();

			while (currentExpression is MethodCallExpression methodCallExpression)
			{
				var stage = ExpressionTranslation.TranslateMethod(methodCallExpression).AsBsonDocument;
				stages.Push(stage);

				currentExpression = methodCallExpression.Arguments[0];
			}

			return stages;
		}
	}
}
