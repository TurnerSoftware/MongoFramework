using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MongoFramework.Infrastructure.Internal;
using MongoFramework.Infrastructure.Linq;

namespace MongoFramework.Infrastructure.Mapping;

public readonly record struct PropertyPath(IReadOnlyList<PropertyInfo> Properties)
{
	/// <summary>
	/// Returns the entity types found through the property path.
	/// </summary>
	/// <returns></returns>
	public IEnumerable<Type> GetEntityTypes()
	{
		foreach (var property in Properties)
		{
			var possibleEntityType = property.PropertyType.UnwrapEnumerableTypes();
			if (EntityMapping.IsValidTypeToMap(possibleEntityType))
			{
				yield return possibleEntityType;
			}
		}
	}

	public bool Contains(PropertyInfo propertyInfo) => Properties.Contains(propertyInfo);

	/// <summary>
	/// Returns a <see cref="PropertyPath"/> based on the resolved properties through the <paramref name="pathExpression"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// For example, take the expression body: <c>v.Thing.Items.First().Name</c><br/>
	/// We want <c>[Thing, Items, Name]</c> but the expression is actually: <c>Name.First().Items.Thing.v</c><br/>
	/// This is also expressed as <c>[MemberExpression, MethodCallExpression, MemberExpression, MemberExpression, ParameterExpression]</c>.
	/// </para>
	/// This is why we have a stack (for our result to be the "correct" order) and we exit on <see cref="ParameterExpression"/>.
	/// </remarks>
	/// <param name="pathExpression"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentException"></exception>
	public static PropertyPath FromExpression(Expression pathExpression)
	{
		var propertyInfoChain = new Stack<PropertyInfo>();
		var current = pathExpression;

		while (current is not ParameterExpression)
		{
			if (current is MemberExpression memberExpression && memberExpression.Member is PropertyInfo propertyInfo)
			{
				propertyInfoChain.Push(propertyInfo);
				current = memberExpression.Expression;
			}
			else if (current is MethodCallExpression methodExpression)
			{
				var genericMethodDefinition = methodExpression.Method.GetGenericMethodDefinition();
				if (genericMethodDefinition == MethodInfoCache.Enumerable.First_1 || genericMethodDefinition == MethodInfoCache.Enumerable.Single_1)
				{
					var callerExpression = methodExpression.Arguments[0];
					current = callerExpression;
				}
				else
				{
					throw new ArgumentException($"Invalid method \"{methodExpression.Method.Name}\". Only \"Enumerable.First()\" and \"Enumerable.Single()\" methods are allowed in chained expressions", nameof(pathExpression));
				}

			}
			else if (current is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert)
			{
				current = unaryExpression.Operand;
			}
			else
			{
				throw new ArgumentException($"Unexpected expression \"{current}\" when processing chained expression", nameof(pathExpression));
			}
		}

		return new(propertyInfoChain.ToArray());
	}

	/// <summary>
	/// Returns a <see cref="PropertyPath"/> based on the resolved properties (by name) through the provided string.
	/// </summary>
	/// <remarks>
	/// For example, take this string: <c>Thing.Items.Name</c><br />
	/// This would be resolved as <c>[Thing, Items, Name]</c> including going through any array/enumerable that might exist.
	/// </remarks>
	/// <param name="propertyPath"></param>
	/// <returns></returns>
	public static PropertyPath FromString(Type baseType, string propertyPath)
	{
		var inputChain = propertyPath.Split('.');
		var propertyInfoChain = new PropertyInfo[inputChain.Length];
		
		var currentType = baseType;
		for (var i = 0; i < inputChain.Length; i++)
		{
			var propertyName = inputChain[i];
			var property = currentType.GetProperty(propertyName) ?? throw new ArgumentException($"Property \"{propertyName}\" is not found on reflected entity types", nameof(propertyPath));
			propertyInfoChain[i] = property;

			var propertyType = property.PropertyType.UnwrapEnumerableTypes();
			currentType = propertyType;
		}

		return new(propertyInfoChain);
	}
}
