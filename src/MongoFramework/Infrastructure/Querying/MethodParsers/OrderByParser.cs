using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MongoDB.Bson;

namespace MongoFramework.Infrastructure.Querying.MethodParsers
{
	public class OrderByParser : IMethodParser
	{
		public static IEnumerable<MethodInfo> GetSupportedMethods()
		{
			yield return ExpressionHelper.GetMethodDefinition(() => Queryable.OrderBy(null, (Expression<Func<object, bool>>)null));
			yield return ExpressionHelper.GetMethodDefinition(() => Queryable.OrderByDescending(null, (Expression<Func<object, bool>>)null));
		}

		public BsonValue ParseMethod(MethodCallExpression expression)
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
							ExpressionParser.BuildPartialQuery(expression.Arguments[1]).AsString,
							direction
						}
					} 
				}
			};
		}
	}
}
