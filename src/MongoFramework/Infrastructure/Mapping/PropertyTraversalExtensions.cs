using System.Collections.Generic;
using System;
using MongoFramework.Infrastructure.Internal;
using System.Linq;
using System.Buffers;
using System.Diagnostics;

namespace MongoFramework.Infrastructure.Mapping;

public interface ITraversedProperty
{
	public ITraversedProperty Parent { get; }
	public IEntityPropertyDefinition Property { get; }
	public int Depth { get; }
	public string GetPath();
}

[DebuggerDisplay("Property = {Property.ElementName}, Parent = {Parent?.Property?.ElementName}, Depth = {Depth}")]
internal record TraversedProperty : ITraversedProperty
{
	private static readonly string ElementSeparator = ".";

	public ITraversedProperty Parent { get; init; }
	public IEntityPropertyDefinition Property { get; init; }
	public int Depth { get; init; }

	public string GetPath()
	{
		if (Depth == 0)
		{
			return Property.ElementName;
		}

		var pool = ArrayPool<string>.Shared.Rent(Depth + 1);
		try
		{
			ITraversedProperty current = this;
			for (var i = Depth; i >= 0; i--)
			{
				pool[i] = current.Property.ElementName;
				current = current.Parent;
			}

			return string.Join(ElementSeparator, pool, 0, Depth + 1);
		}
		finally
		{
			ArrayPool<string>.Shared.Return(pool);
		}
	}
}

public static class PropertyTraversalExtensions
{
	private readonly record struct TraversalState
	{
		public HashSet<Type> SeenTypes { get; init; }
		public IEnumerable<ITraversedProperty> Properties { get; init; }
	}

	public static IEnumerable<ITraversedProperty> TraverseProperties(this IEntityDefinition definition)
	{
		var stack = new Stack<TraversalState>();
		stack.Push(new TraversalState
		{
			SeenTypes = new HashSet<Type> { definition.EntityType },
			Properties = definition.GetAllProperties().Select(p => new TraversedProperty
			{
				Property = p,
				Depth = 0
			})
		});

		while (stack.Count > 0)
		{
			var state = stack.Pop();
			foreach (var traversedProperty in state.Properties)
			{
				yield return traversedProperty;

				var propertyType = traversedProperty.Property.PropertyInfo.PropertyType;
				propertyType = propertyType.GetEnumerableItemTypeOrDefault();

				if (EntityMapping.IsValidTypeToMap(propertyType) && !state.SeenTypes.Contains(propertyType))
				{
					var nestedProperties = EntityMapping.GetOrCreateDefinition(propertyType)
						.GetAllProperties()
						.Select(p => new TraversedProperty
						{
							Parent = traversedProperty,
							Property = p,
							Depth = traversedProperty.Depth + 1
						});

					stack.Push(new TraversalState
					{
						SeenTypes = new HashSet<Type>(state.SeenTypes)
						{
							propertyType
						},
						Properties = nestedProperties
					});
				}
			}
		}
	}
}