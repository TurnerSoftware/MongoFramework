using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MongoDB.Bson;

namespace MongoFramework.Infrastructure.Linq.Translation.Translators
{
	public class TakeTranslator : IMethodTranslator
	{
		public IEnumerable<MethodInfo> GetSupportedMethods()
		{
			yield return TranslationHelper.GetMethodDefinition(() => Queryable.Take((IQueryable<object>)null, 0));
		}

		public BsonValue TranslateMethod(MethodCallExpression expression, IEnumerable<Expression> methodSuffixExpressions = default)
		{
			return new BsonDocument
			{
				{ "$limit", ExpressionTranslation.TranslateConstant(expression.Arguments[1]) }
			};
		}
	}
}
