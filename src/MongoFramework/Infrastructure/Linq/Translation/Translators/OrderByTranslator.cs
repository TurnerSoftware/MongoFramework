using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MongoDB.Bson;

namespace MongoFramework.Infrastructure.Linq.Translation.Translators
{
	public class OrderByTranslator : IMethodTranslator
	{
		public IEnumerable<MethodInfo> GetSupportedMethods()
		{
			yield return TranslationHelper.GetMethodDefinition(() => Queryable.OrderBy(null, (Expression<Func<object, bool>>)null));
			yield return TranslationHelper.GetMethodDefinition(() => Queryable.OrderByDescending(null, (Expression<Func<object, bool>>)null));
		}

		public BsonValue TranslateMethod(MethodCallExpression expression, IEnumerable<Expression> methodSuffixExpressions = default)
		{
			var direction = 1;
			if (expression.Method.Name.EndsWith("Descending"))
			{
				direction = -1;
			}

			return new BsonDocument
			{
				{
					"$sort", 
					new BsonDocument
					{
						{
							ExpressionTranslation.TranslateSubExpression(expression.Arguments[1]).AsString,
							direction
						}
					} 
				}
			};
		}
	}
}
