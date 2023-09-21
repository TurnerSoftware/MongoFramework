using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MongoDB.Bson;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Infrastructure.Linq.Translation
{
	public static class ExpressionTranslation
	{
		private static readonly HashSet<ExpressionType> DefaultSupportedTypes = new HashSet<ExpressionType>
		{
			ExpressionType.Equal,
			ExpressionType.NotEqual,
			ExpressionType.LessThan,
			ExpressionType.GreaterThan,
			ExpressionType.LessThanOrEqual,
			ExpressionType.GreaterThanOrEqual,
			ExpressionType.OrElse,
			ExpressionType.AndAlso,
			ExpressionType.ArrayIndex
		};

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

		private static readonly Dictionary<MethodInfo, IMethodTranslator> MethodTranslatorMap = new Dictionary<MethodInfo, IMethodTranslator>();
		private static readonly Dictionary<MemberInfo, IMemberTranslator> MemberTranslatorMap = new Dictionary<MemberInfo, IMemberTranslator>();
		private static readonly Dictionary<ExpressionType, IBinaryExpressionTranslator> BinaryTranslatorMap = new Dictionary<ExpressionType, IBinaryExpressionTranslator>();

		static ExpressionTranslation()
		{
			DefaultTranslators.AddTranslators();
		}

		public static void AddTranslator(IQueryTranslator translator)
		{
			if (translator is IMethodTranslator methodTranslator)
			{
				lock (MethodTranslatorMap)
				{
					foreach (var method in methodTranslator.GetSupportedMethods())
					{
						MethodTranslatorMap.Add(method, methodTranslator);
					}
				}
			}
			else if (translator is IMemberTranslator memberTranslator)
			{
				lock (MemberTranslatorMap)
				{
					foreach (var member in memberTranslator.GetSupportedMembers())
					{
						MemberTranslatorMap.Add(member, memberTranslator);
					}
				}
			}
			else if (translator is IBinaryExpressionTranslator binaryExpressionTranslator)
			{
				lock (BinaryTranslatorMap)
				{
					foreach (var expressionType in binaryExpressionTranslator.GetSupportedExpressionTypes())
					{
						if (DefaultSupportedTypes.Contains(expressionType))
						{
							throw new ArgumentException($"{expressionType} is a default expression type and can not have a custom translator");
						}

						BinaryTranslatorMap.Add(expressionType, binaryExpressionTranslator);
					}
				}
			}
			else
			{
				throw new ArgumentException($"Invalid type of translator. It must implement {nameof(IMethodTranslator)}, {nameof(IMemberTranslator)} or {nameof(IBinaryExpressionTranslator)}.", nameof(translator));
			}
		}

		public static void ClearTranslators()
		{
			lock (MethodTranslatorMap)
			{
				MethodTranslatorMap.Clear();
			}

			lock (MemberTranslatorMap)
			{
				MemberTranslatorMap.Clear();
			}

			lock (BinaryTranslatorMap)
			{
				BinaryTranslatorMap.Clear();
			}
		}

		public static BsonValue TranslateSubExpression(Expression expression)
		{
			var unwrappedExpression = UnwrapLambda(expression);
			var sourceExpression = GetMemberSource(unwrappedExpression);

			if (sourceExpression is ConstantExpression)
			{
				return TranslateConstant(unwrappedExpression);
			}
			else
			{
				if (
					(unwrappedExpression is BinaryExpression && unwrappedExpression.NodeType == ExpressionType.ArrayIndex) ||
					unwrappedExpression is MemberExpression
				)
				{
					return TranslateMember(unwrappedExpression);
				}
				else if (unwrappedExpression is BinaryExpression binaryExpression && !DefaultSupportedTypes.Contains(unwrappedExpression.NodeType))
				{
					IBinaryExpressionTranslator binaryExpressionTranslator;
					lock (BinaryTranslatorMap)
					{
						if (!BinaryTranslatorMap.TryGetValue(unwrappedExpression.NodeType, out binaryExpressionTranslator))
						{
							throw new ArgumentException($"No binary expression translator found for {unwrappedExpression.NodeType}");
						}
					}

					return binaryExpressionTranslator.TranslateBinary(binaryExpression);
				}
				else if (unwrappedExpression is MethodCallExpression methodCallExpression)
				{
					return TranslateMethod(methodCallExpression);
				}

				throw new ArgumentException($"Unexpected expression type {unwrappedExpression}");
			}
		}

		private static string GetFieldNameFromMember(MemberInfo memberInfo)
		{
			var entityDefinition = EntityMapping.GetOrCreateDefinition(memberInfo.DeclaringType);
			var entityProperty = entityDefinition.GetProperty(memberInfo.Name);
			return entityProperty.ElementName;
		}

		public static BsonString GetFieldName(Expression expression)
		{
			var partialNamePieces = new Stack<string>();
			var currentExpression = expression;

			while (true)
			{
				if (currentExpression is BinaryExpression binaryExpression && currentExpression.NodeType == ExpressionType.ArrayIndex)
				{
					//The index is on the right
					var arrayIndex = TranslateSubExpression(binaryExpression.Right);
					partialNamePieces.Push(arrayIndex.ToString());

					//The parent expression is on the left
					currentExpression = binaryExpression.Left;
				}
				else if (currentExpression is MemberExpression memberExpression)
				{
					var fieldName = GetFieldNameFromMember(memberExpression.Member);
					partialNamePieces.Push(fieldName);

					currentExpression = memberExpression.Expression;
				}
				else if (currentExpression is ParameterExpression || currentExpression is ConstantExpression)
				{
					return string.Join(".", partialNamePieces);
				}
				else
				{
					throw new ArgumentException($"Unexpected node type {currentExpression.NodeType}.");
				}
			}
		}

		public static Expression GetMemberSource(Expression expression)
		{
			var currentExpression = expression;
			while (currentExpression != null)
			{
				if (currentExpression is MemberExpression memberExpression)
				{
					currentExpression = memberExpression.Expression;
				}
				else if (currentExpression is BinaryExpression binaryExpression && binaryExpression.NodeType == ExpressionType.ArrayIndex)
				{
					currentExpression = binaryExpression.Left;
				}
				else if (currentExpression is MethodCallExpression methodCallExpression)
				{
					if (methodCallExpression.Object != null)
					{
						currentExpression = methodCallExpression.Object;
					}
					else if (methodCallExpression.Arguments.Count > 0)
					{
						currentExpression = methodCallExpression.Arguments[0];
					}
					else
					{
						return currentExpression;
					}
				}
				else if (currentExpression is ParameterExpression || currentExpression is ConstantExpression)
				{
					return currentExpression;
				}
				else
				{
					throw new ArgumentException($"Unable to determine source expression for {currentExpression}");
				}
			}

			return currentExpression;
		}

		public static BsonDocument TranslateConditional(Expression expression, bool negated = false)
		{
			var localExpression = UnwrapLambda(expression);

			static void UnwrapBinaryQuery(BsonArray target, ExpressionType expressionType, BinaryExpression expression, bool negated)
			{
				if (expression.Left.NodeType == expressionType)
				{
					UnwrapBinaryQuery(target, expressionType, expression.Left as BinaryExpression, negated);
				}
				else
				{
					target.Add(TranslateConditional(expression.Left, negated));
				}

				target.Add(TranslateConditional(expression.Right, negated));
			}

			if (localExpression is BinaryExpression binaryExpression)
			{
				if (localExpression.NodeType == ExpressionType.AndAlso)
				{
					var unwrappedQuery = new BsonArray();
					UnwrapBinaryQuery(unwrappedQuery, ExpressionType.AndAlso, binaryExpression, negated);

					var elements = new BsonElement[unwrappedQuery.Count];

					for (int i = 0, l = unwrappedQuery.Count; i < l; i++)
					{
						elements[i] = unwrappedQuery[i].AsBsonDocument.GetElement(0);
					}

					return new BsonDocument((IEnumerable<BsonElement>)elements);
				}
				else if (localExpression.NodeType == ExpressionType.OrElse)
				{
					var unwrappedQuery = new BsonArray();
					UnwrapBinaryQuery(unwrappedQuery, ExpressionType.OrElse, binaryExpression, negated);
					return new BsonDocument
					{
						{ "$or", unwrappedQuery }
					};
				}
				else if (ComparatorToStringMap.Keys.Contains(localExpression.NodeType))
				{
					var leftValue = TranslateSubExpression(binaryExpression.Left);
					var rightValue = TranslateSubExpression(binaryExpression.Right);

					string fieldName;
					BsonValue value;
					var expressionType = binaryExpression.NodeType;

					if (binaryExpression.Left.NodeType == ExpressionType.Constant)
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
						fieldName = leftValue.AsString;
						value = rightValue;
					}

					var expressionOperator = ComparatorToStringMap[expressionType];
					var valueComparison = new BsonDocument { { expressionOperator, value } };

					if (negated)
					{
						valueComparison = new BsonDocument
						{
							{ "$not", valueComparison }
						};
					}

					return new BsonDocument { { fieldName, valueComparison } };
				}
			}
			else if (localExpression is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Not)
			{
				if (unaryExpression.Operand.NodeType == ExpressionType.OrElse)
				{
					var translatedInnerExpression = TranslateConditional(unaryExpression.Operand, false);
					var valueItems = translatedInnerExpression.GetElement("$or").Value;

					return new BsonDocument
					{
						{ "$nor", valueItems }
					};
				}
				else
				{
					return TranslateConditional(unaryExpression.Operand, !negated);
				}
			}

			throw new ArgumentException($"Unexpected node type {expression.NodeType} for a conditional statement");
		}

		public static BsonValue TranslateConstant(Expression expression)
		{
			object value;
			if (expression is ConstantExpression constantExpression)
			{
				value = constantExpression.Value;
			}
			else
			{
				var objectMember = Expression.Convert(expression, typeof(object));
				var getterLambda = Expression.Lambda<Func<object>>(objectMember);
				var getter = getterLambda.Compile();
				value = getter();

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

			return BsonValue.Create(value);
		}

		public static BsonDocument TranslateInstantiation(Expression expression)
		{
			var result = new BsonDocument();

			if (expression is MemberInitExpression memberInitExpression)
			{
				for (var i = 0; i < memberInitExpression.Bindings.Count; i++)
				{
					var binding = memberInitExpression.Bindings[i];

					if (binding.BindingType != MemberBindingType.Assignment)
					{
						throw new ArgumentException($"Unexpected binding type {binding.BindingType}", nameof(expression));
					}
					else if (binding is MemberAssignment memberAssignment)
					{
						var mappedName = GetFieldNameFromMember(memberAssignment.Member);
						result.Add(mappedName, TranslateSubExpression(memberAssignment.Expression));
					}
				}
			}
			else if (expression is NewExpression newExpression)
			{
				for (var i = 0; i < newExpression.Members.Count; i++)
				{
					var mappedName = GetFieldNameFromMember(newExpression.Members[i]);
					result.Add(mappedName, TranslateSubExpression(newExpression.Arguments[i]));
				}
			}
			else
			{
				throw new ArgumentException($"Unsupported type of instantiation {expression}");
			}

			return result;
		}

		public static BsonValue TranslateMember(Expression expression)
		{
			var walkedExpressions = new Stack<Expression>();
			var currentExpression = expression;

			while (currentExpression != null)
			{
				if (currentExpression is BinaryExpression binaryExpression && binaryExpression.NodeType == ExpressionType.ArrayIndex)
				{
					walkedExpressions.Push(currentExpression);
					currentExpression = binaryExpression.Left;
				}
				else if (currentExpression is MethodCallExpression methodCallExpression)
				{
					return TranslateMethod(methodCallExpression, walkedExpressions);
				}
				else if (currentExpression is MemberExpression memberExpression)
				{
					IMemberTranslator memberParser;
					lock (MemberTranslatorMap)
					{
						MemberTranslatorMap.TryGetValue(memberExpression.Member, out memberParser);
					}

					if (memberParser != null)
					{
						return memberParser.TranslateMember(memberExpression, walkedExpressions);
					}
					else
					{
						walkedExpressions.Push(currentExpression);
						currentExpression = memberExpression.Expression;
					}
				}
				else if (currentExpression is ParameterExpression)
				{
					return GetFieldName(expression);
				}
				else
				{
					throw new ArgumentException($"Unexpected node type {expression.NodeType}");
				}
			}

			return BsonNull.Value;
		}

		public static BsonValue TranslateMethod(MethodCallExpression expression, IEnumerable<Expression> methodSuffixExpressions = default)
		{
			var methodDefinition = expression.Method;
			if (methodDefinition.IsGenericMethod)
			{
				methodDefinition = methodDefinition.GetGenericMethodDefinition();
			}

			IMethodTranslator methodParser;
			lock (MethodTranslatorMap)
			{
				if (!MethodTranslatorMap.TryGetValue(methodDefinition, out methodParser))
				{
					throw new InvalidOperationException($"No method translator found for {expression.Method}");
				}
			}

			return methodParser.TranslateMethod(expression, methodSuffixExpressions);
		}

		public static Expression UnwrapLambda(Expression expression)
		{
			var localExpression = expression;
			if (localExpression.NodeType == ExpressionType.Quote && localExpression is UnaryExpression unaryExpression)
			{
				localExpression = unaryExpression.Operand;
			}

			if (localExpression.NodeType == ExpressionType.Lambda && localExpression is LambdaExpression lambdaExpression)
			{
				localExpression = lambdaExpression.Body;
			}

			return localExpression;
		}
	}
}
