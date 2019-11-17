using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MongoDB.Bson;

namespace MongoFramework.Infrastructure.Linq.Translation
{
	public interface IQueryTranslator { }
	public interface IMethodTranslator : IQueryTranslator
	{
		IEnumerable<MethodInfo> GetSupportedMethods();
		BsonValue TranslateMethod(MethodCallExpression expression, IEnumerable<Expression> suffixExpressions = default);
	}

	public interface IMemberTranslator : IQueryTranslator
	{
		IEnumerable<MemberInfo> GetSupportedMembers();
		BsonValue TranslateMember(MemberExpression expression, IEnumerable<Expression> suffixExpressions = default);
	}

	public interface IBinaryExpressionTranslator : IQueryTranslator
	{
		IEnumerable<ExpressionType> GetSupportedExpressionTypes();
		BsonValue TranslateBinary(BinaryExpression expression);
	}
}
