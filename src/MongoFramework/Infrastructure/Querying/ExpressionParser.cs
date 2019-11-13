using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MongoDB.Bson;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Querying.MethodParsers;

namespace MongoFramework.Infrastructure.Querying
{
	public static class ExpressionParser
	{
		private static readonly IReadOnlyDictionary<ExpressionType, string> ComparatorToStringMap = new Dictionary<ExpressionType, string>
		{
			{ ExpressionType.Equal, "$eq" },
			{ ExpressionType.NotEqual, "$nq" },
			{ ExpressionType.LessThan, "$lt" },
			{ ExpressionType.GreaterThan, "$gt" },
			{ ExpressionType.LessThanOrEqual, "$lte" },
			{ ExpressionType.GreaterThanOrEqual, "$gte" }
		};

		private static readonly IReadOnlyDictionary<ExpressionType, ExpressionType> NumericComparatorInversionMap = new Dictionary<ExpressionType, ExpressionType>
		{
			{ ExpressionType.LessThan, ExpressionType.GreaterThan },
			{ ExpressionType.GreaterThan, ExpressionType.LessThan },
			{ ExpressionType.LessThanOrEqual, ExpressionType.GreaterThanOrEqual },
			{ ExpressionType.GreaterThanOrEqual, ExpressionType.LessThanOrEqual }
		};

		private static object MethodParserLockObj = new object();

		private static readonly Dictionary<MethodInfo, IMethodParser> MethodParserMap = new Dictionary<MethodInfo, IMethodParser>();

		static ExpressionParser()
		{
			AddMethodParser(new WhereParser(), WhereParser.GetSupportedMethods());
			AddMethodParser(new OrderByParser(), OrderByParser.GetSupportedMethods());
			AddMethodParser(new SelectParser(), SelectParser.GetSupportedMethods());
		}

		public static void AddMethodParser(IMethodParser parser, IEnumerable<MethodInfo> methods)
		{
			lock (MethodParserLockObj)
			{
				foreach (var method in methods)
				{
					MethodParserMap.Add(method, parser);
				}
			}
		}

		public static BsonValue BuildPartialQuery(Expression expression)
		{
			if (expression is BinaryExpression binaryExpression)
			{
				if (expression.NodeType == ExpressionType.AndAlso)
				{
					var unwrappedQuery = new BsonArray();
					UnwrapBinaryQuery(unwrappedQuery, ExpressionType.AndAlso, binaryExpression);

					var elements = new BsonElement[unwrappedQuery.Count];

					for (int i = 0, l = unwrappedQuery.Count; i < l; i++)
					{
						elements[i] = unwrappedQuery[i].AsBsonDocument.GetElement(0);
					}

					return new BsonDocument((IEnumerable<BsonElement>)elements);
				}
				else if (expression.NodeType == ExpressionType.OrElse)
				{
					var unwrappedQuery = new BsonArray();
					UnwrapBinaryQuery(unwrappedQuery, ExpressionType.OrElse, binaryExpression);
					return new BsonDocument
					{
						{ "$or", unwrappedQuery }
					};
				}
				else if (expression.NodeType == ExpressionType.ArrayIndex)
				{
					return BuildMemberAccessQuery(binaryExpression);
				}
				else
				{
					return BuildPartialEquality(binaryExpression);
				}
			}
			else if (expression is MethodCallExpression methodCallExpression)
			{
				return BuildMethodQuery(methodCallExpression);
			}
			else if (expression is ConstantExpression constantExpression)
			{
				return BsonValue.Create(constantExpression.Value);
			}
			else if (expression is MemberExpression memberExpression)
			{
				return BuildMemberAccessQuery(memberExpression);
			}
			else if (expression is NewExpression newExpression)
			{
				return BuildInstantiationQuery(newExpression);
			}
			else if (expression is ParameterExpression)
			{
				return null;
			}
			else if (expression is UnaryExpression unaryExpression)
			{
				if (unaryExpression.NodeType == ExpressionType.Not)
				{
					string operatorName;
					if (unaryExpression.Operand.NodeType == ExpressionType.OrElse)
					{
						operatorName = "$nor";
					}
					else
					{
						operatorName = "$not";
					}

					return new BsonDocument
					{
						{ operatorName, BuildPartialQuery(unaryExpression.Operand) }
					};
				}
				else if (unaryExpression.NodeType == ExpressionType.Quote)
				{
					var nestedLambda = unaryExpression.Operand as LambdaExpression;
					return BuildPartialQuery(nestedLambda.Body);
				}
			}

			throw new ArgumentException($"Unexpected expression type {expression}");
		}

		private static BsonValue BuildInstantiationQuery(NewExpression newExpression)
		{
			var projectionDocument = new BsonDocument
			{
				{ "_id", 0 }
			};

			for (var i = 0; i < newExpression.Members.Count; i++)
			{
				var fromExpression = BuildPartialQuery(newExpression.Arguments[i]);

				var member = newExpression.Members[i];
				var entityDefinition = EntityMapping.GetOrCreateDefinition(member.DeclaringType);
				var entityProperty = entityDefinition.GetProperty(member.Name);

				projectionDocument.Add(entityProperty.ElementName, fromExpression);
			}

			return projectionDocument;
		}

		private static BsonValue BuildMemberAccessQuery(Expression expression)
		{
			var walkedExpressions = new Stack<Expression>();
			var currentUnrolledExpression = expression;

			MethodCallExpression outerMostMethodExpression = null;

			while (true)
			{
				if (currentUnrolledExpression is BinaryExpression binaryExpression)
				{
					if (binaryExpression.NodeType == ExpressionType.ArrayIndex)
					{
						//The index is on the right (a ConstantExpression)
						walkedExpressions.Push(binaryExpression.Right);

						//The parent expression is on the left
						currentUnrolledExpression = binaryExpression.Left;
					}
					else
					{
						throw new ArgumentException($"Unexpected node type {expression.NodeType}. Expected {ExpressionType.ArrayIndex}.");
					}
				}
				else if (currentUnrolledExpression is MethodCallExpression methodCallExpression)
				{
					if (outerMostMethodExpression == null)
					{
						outerMostMethodExpression = methodCallExpression;
					}

					walkedExpressions.Push(methodCallExpression);
					currentUnrolledExpression = methodCallExpression.Object;
				}
				else if (currentUnrolledExpression is MemberExpression memberExpression)
				{
					walkedExpressions.Push(memberExpression);
					currentUnrolledExpression = memberExpression.Expression;
				}
				else if (currentUnrolledExpression is ParameterExpression || currentUnrolledExpression is ConstantExpression)
				{
					walkedExpressions.Push(currentUnrolledExpression);
					break;
				}
				else
				{
					throw new ArgumentException($"Unexpected node type {expression.NodeType}. Expected {ExpressionType.ArrayIndex}.");
				}
			}

			var firstExpression = walkedExpressions.Peek();

			if (firstExpression is ParameterExpression)
			{
				//ParameterExpression means we can't determine the value
				//If we find a method in the path, discard everything after it and treat this as a method
				if (outerMostMethodExpression != null)
				{
					return BuildMethodQuery(outerMostMethodExpression);
				}
				else
				{
					//When no methods are found, treat this as a straight field name
					var namePieces = new List<string>();

					//Remove the parameter expression, we don't need it as part of the field name
					walkedExpressions.Pop();

					foreach (var currentExpression in walkedExpressions)
					{
						if (currentExpression is MemberExpression memberExpression)
						{
							var member = memberExpression.Member;
							var entityDefinition = EntityMapping.GetOrCreateDefinition(member.DeclaringType);
							var entityProperty = entityDefinition.GetProperty(member.Name);
							namePieces.Add(entityProperty.ElementName);
						}
						else if (currentExpression is ConstantExpression constantExpression)
						{
							var arrayIndex = constantExpression.Value.ToString();
							namePieces.Add(arrayIndex);
						}
						else
						{
							throw new ArgumentException($"Unexpected expression type {currentExpression} for a field name.");
						}
					}

					return new BsonString(string.Join(".", namePieces));
				}
			}
			else
			{
				var constantValue = GetValueFromExpression(expression);
				return BsonValue.Create(constantValue);
			}
		}

		private static BsonValue BuildMethodQuery(MethodCallExpression expression)
		{
			var methodDefinition = expression.Method;
			if (methodDefinition.IsGenericMethod)
			{
				methodDefinition = methodDefinition.GetGenericMethodDefinition();
			}

			IMethodParser methodParser;
			lock (MethodParserLockObj)
			{
				if (!MethodParserMap.TryGetValue(methodDefinition, out methodParser))
				{
					throw new InvalidOperationException($"No method parser found for {expression.Method}");
				}
			}

			return methodParser.ParseMethod(expression);
		}

		private static object GetValueFromExpression(Expression expression)
		{
			var objectMember = Expression.Convert(expression, typeof(object));
			var getterLambda = Expression.Lambda<Func<object>>(objectMember);
			var getter = getterLambda.Compile();
			return getter();

			//TODO: Leaving the code below as it might perform faster than compiling the lambda

			//var expressionStack = new Stack<Expression>(expressions);
			//var currentValue = (expressionStack.Pop() as ConstantExpression).Value;

			//Expression currentExpression;
			//while ((currentExpression = expressionStack.Pop()) != null)
			//{
			//	if (currentExpression is MemberExpression memberExpression)
			//	{
			//		var memberInfo = memberExpression.Member;
			//		if (memberInfo is PropertyInfo propertyInfo)
			//		{
			//			if (expressionStack.Peek() is ConstantExpression constantExpression)
			//			{
			//				currentValue = propertyInfo.GetValue(currentValue, new[] { });
			//			}
			//			else
			//			{
			//				currentValue = propertyInfo.GetValue(currentValue);
			//			}
			//		}
			//		else if (memberInfo is FieldInfo fieldInfo)
			//		{
			//			currentValue = fieldInfo.GetValue(currentValue);
			//		}
			//	}
			//	else
			//	{
			//		var constantExpression = currentExpression as ConstantExpression;
			//		constantExpression.
			//		//TODO: get value from the array index
			//	}
			//}
		}

		private static BsonValue BuildPartialEquality(BinaryExpression binaryExpression)
		{
			var leftValue = BuildPartialQuery(binaryExpression.Left);
			var rightValue = BuildPartialQuery(binaryExpression.Right);

			string fieldName;
			BsonValue value;
			var expressionType = binaryExpression.NodeType;

			if (binaryExpression.Left.NodeType != ExpressionType.MemberAccess)
			{
				if (binaryExpression.Right.NodeType == ExpressionType.MemberAccess)
				{
					//For expressions like "3 < myEntity.MyValue", we need to flip that around
					//This flip is because the field name is "left" of the value
					//When flipping it, we need to invert the expression to "myEntity.MyValue > 3"

					fieldName = rightValue.AsString;
					value = leftValue;

					if (expressionType != ExpressionType.Equal && expressionType != ExpressionType.NotEqual)
					{
						expressionType = NumericComparatorInversionMap[expressionType];
					}
				}
				else
				{
					throw new ArgumentException($"Expected expression type {ExpressionType.MemberAccess} but received {binaryExpression.Right.NodeType}");
				}
			}
			else
			{
				fieldName = leftValue.AsString;
				value = rightValue;
			}

			var expressionOperator = ComparatorToStringMap[expressionType];
			var valueComparison = new BsonDocument { { expressionOperator, value } };
			return new BsonDocument { { fieldName, valueComparison } };
		}

		private static void UnwrapBinaryQuery(BsonArray target, ExpressionType expressionType, BinaryExpression expression)
		{
			if (expression.Left.NodeType == expressionType)
			{
				UnwrapBinaryQuery(target, expressionType, expression.Left as BinaryExpression);
			}
			else
			{
				target.Add(BuildPartialQuery(expression.Left));
			}

			target.Add(BuildPartialQuery(expression.Right));
		}
	}
}
