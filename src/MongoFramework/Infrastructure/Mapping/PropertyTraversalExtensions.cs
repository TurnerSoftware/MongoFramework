using System.Collections.Generic;
using System;
using MongoFramework.Infrastructure.Internal;
using System.Linq;
using System.Buffers;
using System.Diagnostics;

namespace MongoFramework.Infrastructure.Mapping;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed record TraversedProperty
{
	private static readonly string ElementSeparator = ".";

	public TraversedProperty Parent { get; init; }
	public PropertyDefinition Property { get; init; }
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
			var current = this;
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

	private string DebuggerDisplay => $"Property = {Property.ElementName}, Parent = {Parent?.Property?.ElementName}, Depth = {Depth}";
}

public static class PropertyTraversalExtensions
{
	private readonly record struct TraversalState
	{
		public HashSet<Type> SeenTypes { get; init; }
		public IEnumerable<TraversedProperty> Properties { get; init; }
	}

	public static IEnumerable<TraversedProperty> TraverseProperties(this EntityDefinition definition)
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
				propertyType = propertyType.ElideEnumerableTypes();

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