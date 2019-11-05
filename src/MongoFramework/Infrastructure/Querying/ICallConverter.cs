using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using MongoDB.Bson;

namespace MongoFramework.Infrastructure.Querying
{
	public interface ICallConverter
	{
		BsonDocument Convert(MethodCallExpression expression);
	}
}
