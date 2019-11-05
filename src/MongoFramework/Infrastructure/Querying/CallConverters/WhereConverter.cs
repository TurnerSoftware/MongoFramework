using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using MongoDB.Bson;

namespace MongoFramework.Infrastructure.Querying.CallConverters
{
	public class WhereConverter : ICallConverter
	{
		public static ICallConverter Instance { get; } = new WhereConverter();

		public BsonDocument Convert(MethodCallExpression expression)
		{
			return new BsonDocument
			{
				{ "$match", ExpressionHelper.Where(expression.Arguments[1]) }
			};
		}
	}
}
