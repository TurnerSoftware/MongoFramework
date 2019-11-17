using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MongoDB.Bson;

namespace MongoFramework.Infrastructure.Linq.Translation.Translators
{
	public class WhereTranslator : IMethodTranslator
	{
		public IEnumerable<MethodInfo> GetSupportedMethods()
		{
			yield return TranslationHelper.GetMethodDefinition(() => Queryable.Where(null, (Expression<Func<object, bool>>)null));
		}

		public BsonValue TranslateMethod(MethodCallExpression expression, IEnumerable<Expression> methodSuffixExpressions = default)
		{
			return new BsonDocument
			{
				{ "$match", ExpressionTranslation.TranslateConditional(expression.Arguments[1]) }
			};
		}
	}
}
