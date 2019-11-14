using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using MongoDB.Bson;

namespace MongoFramework.Infrastructure.Querying
{
	public interface IMethodTranslator
	{
		BsonValue TranslateMethod(MethodCallExpression expression, IEnumerable<Expression> suffixExpressions = default);
	}

	public interface IMemberTranslator
	{
		BsonValue TranslateMember(MemberExpression expression, IEnumerable<Expression> suffixExpressions = default);
	}

	public interface IBinaryExpressionTranslator
	{
		BsonValue TranslateBinary(BinaryExpression expression);
	}
}
