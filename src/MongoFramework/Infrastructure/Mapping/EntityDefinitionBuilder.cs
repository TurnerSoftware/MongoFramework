using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MongoFramework.Infrastructure.Mapping;

public class EntityDefinitionBuilder
{
	public Type EntityType { get; private set; }
	public string CollectionName { get; private set; }
	public PropertyInfo ExtraElementsProperty { get; private set; }
	public EntityKeyBuilder KeyBuilder { get; private set; }


	private readonly Dictionary<PropertyInfo, EntityPropertyBuilder> propertyBuilders;
	public IReadOnlyCollection<EntityPropertyBuilder> Properties => propertyBuilders.Values;


	private readonly List<EntityIndexBuilder> indexBuilders;
	public IReadOnlyCollection<EntityIndexBuilder> Indexes => indexBuilders;

	public MappingBuilder MappingBuilder { get; }

	public EntityDefinitionBuilder(Type entityType, MappingBuilder mappingBuilder)
	{
		EntityType = entityType;
		MappingBuilder = mappingBuilder;
		propertyBuilders = new();
		indexBuilders = new();
	}

	public EntityDefinitionBuilder(EntityDefinitionBuilder definitionBuilder)
	{
		EntityType = definitionBuilder.EntityType;
		MappingBuilder = definitionBuilder.MappingBuilder;
		CollectionName = definitionBuilder.CollectionName;
		ExtraElementsProperty = definitionBuilder.ExtraElementsProperty;
		KeyBuilder = definitionBuilder.KeyBuilder;
		propertyBuilders = definitionBuilder.propertyBuilders;
		indexBuilders = definitionBuilder.indexBuilders;
	}

	private static void CheckPropertyReadWrite(PropertyInfo propertyInfo)
	{
		if (!propertyInfo.CanWrite || !propertyInfo.CanRead)
		{
			throw new ArgumentException($"Property \"{propertyInfo.Name}\" must be both readable and writeable.", nameof(propertyInfo));
		}
	}

	public EntityDefinitionBuilder ToCollection(string collectionName)
	{
		CollectionName = collectionName;
		return this;
	}

	private void ApplyKeyBuilder(EntityPropertyBuilder propertyBuilder)
	{
		KeyBuilder = new(propertyBuilder.PropertyInfo, propertyBuilder);
	}
	public EntityDefinitionBuilder HasKey(PropertyInfo propertyInfo, Action<EntityKeyBuilder> builder = null)
	{
		HasProperty(propertyInfo, ApplyKeyBuilder);
		builder?.Invoke(KeyBuilder);
		return this;
	}

	public EntityDefinitionBuilder Ignore(PropertyInfo propertyInfo)
	{
		if (propertyInfo.DeclaringType != EntityType)
		{
			throw new ArgumentException($"Can not ignore properties that aren't declared on \"{EntityType.Name}\"", nameof(propertyInfo));
		}

		propertyBuilders.Remove(propertyInfo);

		if (KeyBuilder is not null && KeyBuilder.Property == propertyInfo)
		{
			KeyBuilder = null;
		}

		if (ExtraElementsProperty == propertyInfo)
		{
			IgnoreExtraElements();
		}

		indexBuilders.RemoveAll(b => b.ContainsProperty(propertyInfo));

		return this;
	}

	public EntityDefinitionBuilder HasProperty(PropertyInfo propertyInfo, Action<EntityPropertyBuilder> builder = null)
	{
		if (propertyInfo.DeclaringType != EntityType)
		{
			throw new ArgumentException($"You can only map properties that are declared on the type you're building for. Property \"{propertyInfo.Name}\" is not declared on \"{EntityType.Name}\".", nameof(propertyInfo));
		}

		CheckPropertyReadWrite(propertyInfo);

		if (!propertyBuilders.TryGetValue(propertyInfo, out var propertyBuilder))
		{
			propertyBuilder = new EntityPropertyBuilder(propertyInfo);
			propertyBuilders[propertyInfo] = propertyBuilder;
		}

		builder?.Invoke(propertyBuilder);
		return this;
	}

	public EntityDefinitionBuilder HasIndex(IEnumerable<IndexProperty> indexProperties, Action<EntityIndexBuilder> builder = null)
	{
		//Ensure all properties of the indexes are mapped
		foreach (var indexProperty in indexProperties)
		{
			foreach (var property in indexProperty.PropertyPath.Properties)
			{
				if (property.DeclaringType == EntityType)
				{
					HasProperty(property);
				}
				else
				{
					MappingBuilder.Entity(property.DeclaringType).HasProperty(property);
				}
			}
		}

		var indexBuilder = new EntityIndexBuilder(indexProperties);
		indexBuilders.Add(indexBuilder);

		builder?.Invoke(indexBuilder);
		return this;
	}
	
	public EntityDefinitionBuilder HasExtraElements(PropertyInfo propertyInfo)
	{
		if (!typeof(IDictionary<string, object>).IsAssignableFrom(propertyInfo.PropertyType))
		{
			throw new ArgumentException($"Property \"{propertyInfo.Name}\" must be assignable to \"IDictionary<string, object>\".", nameof(propertyInfo));
		}

		HasProperty(propertyInfo);

		ExtraElementsProperty = propertyInfo;
		return this;
	}

	public EntityDefinitionBuilder IgnoreExtraElements()
	{
		ExtraElementsProperty = null;
		return this;
	}

	public EntityDefinitionBuilder WithDerivedEntity(Type derivedType, Action<EntityDefinitionBuilder> builder)
	{
		if (!derivedType.IsAssignableFrom(derivedType))
		{
			throw new ArgumentException($"Type \"{derivedType}\" is not assignable from \"{EntityType}\"");
		}

		var definitionBuilder = MappingBuilder.Entity(derivedType);
		builder(definitionBuilder);
		return this;
	}
}

public class EntityDefinitionBuilder<TEntity> : EntityDefinitionBuilder
{
	public EntityDefinitionBuilder(MappingBuilder mappingBuilder) : base(typeof(TEntity), mappingBuilder) { }

	private EntityDefinitionBuilder(EntityDefinitionBuilder definitionBuilder) : base(definitionBuilder) { }
	public static EntityDefinitionBuilder<TEntity> CreateFrom(EntityDefinitionBuilder definitionBuilder)
	{
		if (typeof(TEntity) != definitionBuilder.EntityType)
		{
			throw new ArgumentException("Mismatched entity types when creating a generic entity definition", nameof(definitionBuilder));
		}

		return new(definitionBuilder);
	}

	private static PropertyInfo GetPropertyInfo<TValue>(Expression<Func<TEntity, TValue>> propertyExpression)
	{
		var unwrappedExpression = UnwrapExpression(propertyExpression.Body);
		if (unwrappedExpression is not MemberExpression memberExpression)
		{
			throw new ArgumentException("Must be a member expression", nameof(propertyExpression));
		}

		if (memberExpression.Member is not PropertyInfo propertyInfo)
		{
			throw new ArgumentException("Must be an expression to a property", nameof(propertyExpression));
		}

		return propertyInfo;
	}

	private static Expression UnwrapExpression(Expression expression)
	{
		if (expression is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert)
		{
			return unaryExpression.Operand;
		}
		return expression;
	}

	public new EntityDefinitionBuilder<TEntity> ToCollection(string collectionName) => base.ToCollection(collectionName) as EntityDefinitionBuilder<TEntity>;

	public EntityDefinitionBuilder<TEntity> HasKey(Expression<Func<TEntity, object>> propertyExpression, Action<EntityKeyBuilder> builder = null)
		=> HasKey(GetPropertyInfo(propertyExpression), builder) as EntityDefinitionBuilder<TEntity>;

	public EntityDefinitionBuilder<TEntity> Ignore(Expression<Func<TEntity, object>> propertyExpression)
		=> Ignore(GetPropertyInfo(propertyExpression)) as EntityDefinitionBuilder<TEntity>;

	public EntityDefinitionBuilder<TEntity> HasProperty(Expression<Func<TEntity, object>> propertyExpression, Action<EntityPropertyBuilder> builder = null)
		=> HasProperty(GetPropertyInfo(propertyExpression), builder) as EntityDefinitionBuilder<TEntity>;

	public EntityDefinitionBuilder<TEntity> HasIndex(Expression<Func<TEntity, object>> indexExpression, Action<EntityIndexBuilder> builder = null)
	{
		var unwrappedExpression = UnwrapExpression(indexExpression.Body);
		if (unwrappedExpression is MemberExpression memberExpression)
		{
			var properties = new[] { new IndexProperty(PropertyPath.FromExpression(memberExpression)) };
			return HasIndex(properties, builder) as EntityDefinitionBuilder<TEntity>;
		}
		else if (unwrappedExpression is NewExpression newObjExpression)
		{
			var properties = new List<IndexProperty>();
			foreach (var expression in newObjExpression.Arguments)
			{
				var propertyInfoChain = PropertyPath.FromExpression(expression);
				properties.Add(new IndexProperty(propertyInfoChain));
			}
			return HasIndex(properties, builder) as EntityDefinitionBuilder<TEntity>;
		}
		else
		{
			throw new ArgumentException("Must be a member expression to a property or a new expression to bind multiple properties as a single index", nameof(indexExpression));
		}
	}

	public EntityDefinitionBuilder<TEntity> HasExtraElements(Expression<Func<TEntity, IDictionary<string, object>>> propertyExpression)
		=> HasExtraElements(GetPropertyInfo(propertyExpression)) as EntityDefinitionBuilder<TEntity>;

	public new EntityDefinitionBuilder<TEntity> IgnoreExtraElements() => base.IgnoreExtraElements() as EntityDefinitionBuilder<TEntity>;

	public EntityDefinitionBuilder<TEntity> WithDerivedEntity<TDerived>(Action<EntityDefinitionBuilder<TDerived>> builder)
	{
		if (!typeof(TEntity).IsAssignableFrom(typeof(TDerived)))
		{
			throw new ArgumentException($"Type \"{typeof(TDerived)}\" is not assignable from \"{typeof(TEntity)}\"");
		}

		var definitionBuilder = MappingBuilder.Entity<TDerived>();
		builder(definitionBuilder);
		return this;
	}
}

public sealed class EntityPropertyBuilder
{
	public PropertyInfo PropertyInfo { get; }
	public string ElementName { get; private set; }

	public EntityPropertyBuilder(PropertyInfo propertyInfo)
	{
		PropertyInfo = propertyInfo;
		ElementName = propertyInfo.Name;
	}

	public EntityPropertyBuilder HasElementName(string elementName)
	{
		ElementName = elementName;
		return this;
	}
}

public readonly record struct IndexProperty(PropertyPath PropertyPath, IndexType IndexType, IndexSortOrder SortOrder)
{
	public IndexProperty(PropertyPath propertyPath) : this(propertyPath, IndexType.Standard, IndexSortOrder.Ascending) { }
}

public sealed class EntityIndexBuilder
{
	private readonly IndexProperty[] indexProperties;
	public IReadOnlyList<IndexProperty> Properties => indexProperties;

	public string IndexName { get; private set; }
	public bool Unique { get; private set; }

	public bool TenantExclusive { get; private set; }

	public EntityIndexBuilder(IEnumerable<IndexProperty> properties)
	{
		indexProperties = properties.ToArray();
	}

	public bool ContainsProperty(PropertyInfo propertyInfo) => Properties.Any(p => p.PropertyPath.Contains(propertyInfo));

	public EntityIndexBuilder HasName(string indexName)
	{
		IndexName = indexName;
		return this;
	}

	public EntityIndexBuilder HasType(params IndexType[] indexTypes)
	{
		if (indexTypes.Length > Properties.Count)
		{
			throw new ArgumentException("Too many items in list of descending indexes", nameof(indexTypes));
		}

		for (var i = 0; i < indexTypes.Length; i++)
		{
			indexProperties[i] = indexProperties[i] with
			{
				IndexType = indexTypes[i]
			};
		}

		return this;
	}

	public EntityIndexBuilder IsDescending(params bool[] descending)
	{
		if (descending.Length > Properties.Count)
		{
			throw new ArgumentException("Too many items in list of descending indexes", nameof(descending));
		}

		for (var i = 0; i < descending.Length; i++)
		{
			indexProperties[i] = indexProperties[i] with
			{
				SortOrder = descending[i] ? IndexSortOrder.Descending : IndexSortOrder.Ascending
			};
		}

		return this;
	}

	public EntityIndexBuilder IsUnique(bool unique = true)
	{
		Unique = unique;
		return this;
	}

	public EntityIndexBuilder IsTenantExclusive(bool tenantExclusive = true)
	{
		TenantExclusive = tenantExclusive;
		return this;
	}
}

public sealed class EntityKeyBuilder
{
	private readonly EntityPropertyBuilder propertyBuilder;

	public PropertyInfo Property { get; }
	public IEntityKeyGenerator KeyGenerator { get; private set; }

	public EntityKeyBuilder(PropertyInfo property, EntityPropertyBuilder propertyBuilder)
	{
		Property = property;
		this.propertyBuilder = propertyBuilder;
	}

	public EntityKeyBuilder HasKeyGenerator(IEntityKeyGenerator keyGenerator)
	{
		KeyGenerator = keyGenerator;
		return this;
	}

	public EntityKeyBuilder WithProperty(Action<EntityPropertyBuilder> builder)
	{
		builder(propertyBuilder);
		return this;
	}
}