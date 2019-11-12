using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MongoDB.Bson;

namespace MongoFramework.Infrastructure.Querying.MethodParsers
{
	public class WhereParser : IMethodParser
	{
		public static IEnumerable<MethodInfo> GetSupportedMethods()
		{
			yield return ExpressionHelper.GetMethodDefinition(() => Queryable.Where(null, (Expression<Func<object, bool>>)null));
		}

		public BsonValue ParseMethod(MethodCallExpression expression)
		{
			return new BsonDocument
			{
				{ "$match", ExpressionParser.BuildPartialQuery(expression.Arguments[1]) }
			};
		}
	}
}
