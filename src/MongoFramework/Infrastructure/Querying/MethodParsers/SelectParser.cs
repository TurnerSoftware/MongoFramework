using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MongoDB.Bson;

namespace MongoFramework.Infrastructure.Querying.MethodParsers
{
	public class SelectParser : IMethodParser
	{
		public static IEnumerable<MethodInfo> GetSupportedMethods()
		{
			yield return ExpressionHelper.GetMethodDefinition(() => Queryable.Select(null, (Expression<Func<object, object>>)null));
		}

		public BsonValue ParseMethod(MethodCallExpression expression)
		{
			return new BsonDocument
			{
				{
					"$project", 
					new BsonDocument
					{
						{ 
							ExpressionParser.BuildPartialQuery(expression.Arguments[1]).AsString,
							""
						}
					} 
				}
			};
		}
	}
}
