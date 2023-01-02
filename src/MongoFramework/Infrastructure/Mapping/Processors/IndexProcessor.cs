using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoFramework.Infrastructure.Internal;
using MongoFramework.Attributes;

namespace MongoFramework.Infrastructure.Mapping.Processors;

//TODO: This needs another revision as it is a mess
public class IndexProcessor : IMappingProcessor
{
	internal record TraversedProperty
	{
		public TraversedProperty Parent { get; init; }
		public EntityPropertyBuilder Property { get; init; }
		public int Depth { get; init; }

		public PropertyPath GetPropertyPath()
		{
			if (Depth == 0)
			{
				return new PropertyPath(new[] { Property.PropertyInfo });
			}

			var path = new PropertyInfo[Depth + 1];
			var current = this;
			for (var i = Depth; i >= 0; i--)
			{
				path[i] = current.Property.PropertyInfo;
				current = current.Parent;
			}

			return new PropertyPath(path);
		}
	}
	private readonly record struct TraversalState
	{
		public HashSet<Type> SeenTypes { get; init; }
		public IEnumerable<TraversedProperty> Properties { get; init; }
	}

	private IEnumerable<EntityPropertyBuilder> GetAllProperties(EntityDefinitionBuilder definitionBuilder)
	{
		var mappingBuilder = definitionBuilder.MappingBuilder;
		var baseType = definitionBuilder.EntityType.BaseType;
		if (baseType != typeof(object) && baseType is not null)
		{
			var baseDefinitionBuilder = mappingBuilder.Entity(baseType);
			foreach (var property in GetAllProperties(baseDefinitionBuilder))
			{
				yield return property;
			}
		}

		foreach (var property in definitionBuilder.Properties)
		{
			yield return property;
		}
	}

	private IEnumerable<TraversedProperty> TraverseProperties(EntityDefinitionBuilder definitionBuilder)
	{
		var mappingBuilder = definitionBuilder.MappingBuilder;

		var stack = new Stack<TraversalState>();
		stack.Push(new TraversalState
		{
			SeenTypes = new HashSet<Type> { definitionBuilder.EntityType },
			Properties = GetAllProperties(definitionBuilder).Select(p => new TraversedProperty
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
					var propertyDefinitionBuilder = mappingBuilder.Entity(propertyType);
					var nestedProperties = GetAllProperties(propertyDefinitionBuilder)
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

	public void ApplyMapping(EntityDefinitionBuilder definitionBuilder)
	{
		var indexTracker = new Dictionary<string, List<(TraversedProperty TraversedProperty, IndexAttribute IndexAttribute)>>();

		//Find all index attributes (with their traversed property path) and group them by their name
		foreach (var traversedProperty in TraverseProperties(definitionBuilder))
		{
			foreach (var indexAttribute in traversedProperty.Property.PropertyInfo.GetCustomAttributes<IndexAttribute>())
			{
				var indexName = indexAttribute.Name ?? string.Empty;
				if (!indexTracker.TryGetValue(indexName, out var traversedProperties))
				{
					traversedProperties = new();
					indexTracker[indexName] = traversedProperties;
				}

				traversedProperties.Add((traversedProperty, indexAttribute));
			}
		}

		//Process the unnamed group as individual indexes
		if (indexTracker.TryGetValue(string.Empty, out var ungroupedIndexes))
		{
			foreach (var ungroupedIndex in ungroupedIndexes)
			{
				var indexAttr = ungroupedIndex.IndexAttribute;
				var indexProperty = new IndexProperty(
					ungroupedIndex.TraversedProperty.GetPropertyPath(),
					indexAttr.IndexType,
					indexAttr.SortOrder
				);
				definitionBuilder.HasIndex(new[] { indexProperty }, b => b
					.IsUnique(indexAttr.IsUnique)
					.IsTenantExclusive(indexAttr.IsTenantExclusive)
				);
			}
			indexTracker.Remove(string.Empty);
		}
		
		//Using the grouped indexes, apply them to the entity definition builder
		foreach (var index in indexTracker)
		{
			var indexName = index.Key;
			var indexProperties = index.Value
				.OrderBy(p => p.IndexAttribute.IndexPriority)
				.Select(p => new IndexProperty(
					p.TraversedProperty.GetPropertyPath(),
					p.IndexAttribute.IndexType,
					p.IndexAttribute.SortOrder
				)).ToArray();
			var indexAttr = index.Value[0].IndexAttribute;
			definitionBuilder.HasIndex(indexProperties, b => b
				.HasName(indexName)
				.IsUnique(indexAttr.IsUnique)
				.IsTenantExclusive(indexAttr.IsTenantExclusive)
			);
		}
	}
}
