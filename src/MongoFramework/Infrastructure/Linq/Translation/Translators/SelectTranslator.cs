using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MongoDB.Bson;

namespace MongoFramework.Infrastructure.Linq.Translation.Translators
{
	public class SelectTranslator : IMethodTranslator
	{
		public IEnumerable<MethodInfo> GetSupportedMethods()
		{
			yield return TranslationHelper.GetMethodDefinition(() => Queryable.Select(null, (Expression<Func<object, object>>)null));
		}

		public BsonValue TranslateMethod(MethodCallExpression expression, IEnumerable<Expression> methodSuffixExpressions = default)
		{
			return new BsonDocument
			{
				{
					"$project",
					ParseExpression(expression.Arguments[1])
				}
			};
		}

		private BsonDocument ParseExpression(Expression expression)
		{
			var localExpression = ExpressionTranslation.UnwrapLambda(expression);

			BsonDocument document;
			if (localExpression is MemberExpression memberExpression)
			{
				//eg: Select(e => e.MyProperty.CanBeNested)
				var fieldName = ExpressionTranslation.GetFieldName(memberExpression).AsString;
				document = new BsonDocument
				{
					{ fieldName, "$" + fieldName }
				};
			}
			else
			{
				//eg: Select(e => new { MyProperty = e.SomeOtherProperty.CanBeNested })
				//eg: Select(e => new SomeKnownType { MyProperty = e.SomeOtherProperty.CanBeNested })
				document = ExpressionTranslation.TranslateInstantiation(localExpression);
			}

			document.Add("_id", 0);
			return document;
		}
	}
}
