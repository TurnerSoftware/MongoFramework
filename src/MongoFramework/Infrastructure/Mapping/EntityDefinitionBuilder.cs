using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MongoFramework.Infrastructure.Internal;
using MongoFramework.Infrastructure.Linq;

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

	private PropertyInfo GetPropertyInfo(string propertyName)
	{
		var propertyInfo = EntityType.GetProperty(propertyName) ?? throw new ArgumentException($"Property \"{propertyName}\" can not be found on \"{EntityType.Name}\".", nameof(propertyName));
		return propertyInfo;
	}

	public EntityDefinitionBuilder ToCollection(string collectionName)
	{
		CollectionName = collectionName;
		return this;
	}

	public EntityDefinitionBuilder HasKey(string propertyName, Action<EntityKeyBuilder> builder) => HasKey(GetPropertyInfo(propertyName), builder);
	public EntityDefinitionBuilder HasKey(PropertyInfo propertyInfo, Action<EntityKeyBuilder> builder)
	{
		if (!propertyInfo.DeclaringType.IsAssignableFrom(EntityType))
		{
			throw new ArgumentException($"Property \"{propertyInfo.Name}\" is not accessible from \"{EntityType.Name}\".", nameof(propertyInfo));
		}

		CheckPropertyReadWrite(propertyInfo);
		KeyBuilder = new EntityKeyBuilder(propertyInfo);

		builder(KeyBuilder);
		return this;
	}

	public EntityDefinitionBuilder Ignore(string propertyName) => Ignore(GetPropertyInfo(propertyName));
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

	public EntityDefinitionBuilder HasProperty(string propertyName, Action<EntityPropertyBuilder> builder) => HasProperty(GetPropertyInfo(propertyName), builder);
	public EntityDefinitionBuilder HasProperty(PropertyInfo propertyInfo, Action<EntityPropertyBuilder> builder)
	{
		if (propertyInfo.DeclaringType != EntityType)
		{
			throw new ArgumentException($"Property \"{propertyInfo.Name}\" must be declared on \"{EntityType.Name}\".", nameof(propertyInfo));
		}

		CheckPropertyReadWrite(propertyInfo);

		if (!propertyBuilders.TryGetValue(propertyInfo, out var propertyBuilder))
		{
			propertyBuilder = new EntityPropertyBuilder(propertyInfo);
			propertyBuilders[propertyInfo] = propertyBuilder;
		}

		builder(propertyBuilder);
		return this;
	}

	public EntityDefinitionBuilder HasIndex(IEnumerable<string> propertyPaths, Action<EntityIndexBuilder> builder)
	{
		var properties = new List<PropertyPath>();
		foreach (var propertyPath in propertyPaths)
		{
			properties.Add(PropertyPath.FromString(EntityType, propertyPath));
		}

		return HasIndex(properties, builder);
	}
	public EntityDefinitionBuilder HasIndex(IEnumerable<PropertyPath> properties, Action<EntityIndexBuilder> builder)
	{
		return HasIndex(properties.Select(p => new IndexProperty(p, IndexType.Standard, IndexSortOrder.Ascending)), builder);
	}
	public EntityDefinitionBuilder HasIndex(IEnumerable<IndexProperty> indexProperties, Action<EntityIndexBuilder> builder)
	{
		var indexBuilder = new EntityIndexBuilder(indexProperties);
		indexBuilders.Add(indexBuilder);

		builder(indexBuilder);
		return this;
	}

	public EntityDefinitionBuilder HasExtraElements(string propertyName) => HasExtraElements(GetPropertyInfo(propertyName));
	public EntityDefinitionBuilder HasExtraElements(PropertyInfo propertyInfo)
	{
		CheckPropertyReadWrite(propertyInfo);
		
		if (!typeof(IDictionary<string, object>).IsAssignableFrom(propertyInfo.PropertyType))
		{
			throw new ArgumentException($"Property \"{propertyInfo.Name}\" must be assignable to \"IDictionary<string, object>\".", nameof(propertyInfo));
		}

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

	private static PropertyInfo GetPropertyInfo(Expression<Func<TEntity, object>> propertyExpression)
	{
		if (propertyExpression.Body is not MemberExpression memberExpression)
		{
			throw new ArgumentException("Must be a member expression", nameof(propertyExpression));
		}

		if (memberExpression.Member is not PropertyInfo propertyInfo)
		{
			throw new ArgumentException("Must be an expression to a property", nameof(propertyExpression));
		}

		return propertyInfo;
	}

	public new EntityDefinitionBuilder<TEntity> ToCollection(string collectionName) => base.ToCollection(collectionName) as EntityDefinitionBuilder<TEntity>;

	public EntityDefinitionBuilder<TEntity> HasKey(Expression<Func<TEntity, object>> propertyExpression, Action<EntityKeyBuilder> builder)
		=> HasKey(GetPropertyInfo(propertyExpression), builder) as EntityDefinitionBuilder<TEntity>;

	public EntityDefinitionBuilder<TEntity> Ignore(Expression<Func<TEntity, object>> propertyExpression)
		=> Ignore(GetPropertyInfo(propertyExpression)) as EntityDefinitionBuilder<TEntity>;

	public EntityDefinitionBuilder<TEntity> HasProperty(Expression<Func<TEntity, object>> propertyExpression, Action<EntityPropertyBuilder> builder)
		=> HasProperty(GetPropertyInfo(propertyExpression), builder) as EntityDefinitionBuilder<TEntity>;

	public EntityDefinitionBuilder<TEntity> HasIndex(Expression<Func<TEntity, object>> indexExpression, Action<EntityIndexBuilder> builder)
	{
		if (indexExpression.Body is MemberExpression memberExpression)
		{
			var properties = new[] { PropertyPath.FromExpression(memberExpression) };
			return HasIndex(properties, builder) as EntityDefinitionBuilder<TEntity>;
		}
		else if (indexExpression.Body is NewExpression newObjExpression)
		{
			var properties = new List<PropertyPath>();
			foreach (var expression in newObjExpression.Arguments)
			{
				var propertyInfoChain = PropertyPath.FromExpression(expression);
				properties.Add(propertyInfoChain);
			}
			return HasIndex(properties, builder) as EntityDefinitionBuilder<TEntity>;
		}
		else
		{
			throw new ArgumentException("Must be a member expression to a property or a new expression to bind multiple properties as a single index", nameof(indexExpression));
		}
	}

	public EntityDefinitionBuilder<TEntity> HasExtraElements(Expression<Func<TEntity, object>> propertyExpression)
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
	}

	public EntityPropertyBuilder HasElementName(string elementName)
	{
		ElementName = elementName;
		return this;
	}
}

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
			var possibleEntityType = property.PropertyType.ElideEnumerableTypes();
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
				if (methodExpression.Method == MethodInfoCache.Enumerable.First_1 || methodExpression.Method == MethodInfoCache.Enumerable.Single_1)
				{
					var callerExpression = methodExpression.Arguments[0];
					current = callerExpression;
				}
				else
				{
					throw new ArgumentException($"Invalid method \"{methodExpression.Method.Name}\". Only \"Enumerable.First()\" and \"Enumerable.Single()\" methods are allowed in chained expressions", nameof(pathExpression));
				}

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

			var propertyType = property.PropertyType.ElideEnumerableTypes();
			currentType = propertyType;
		}

		return new(propertyInfoChain);
	}
}

public readonly record struct IndexProperty(PropertyPath PropertyPath, IndexType IndexType, IndexSortOrder SortOrder);
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
	public PropertyInfo Property { get; }
	public IEntityKeyGenerator KeyGenerator { get; private set; }

	public EntityKeyBuilder(PropertyInfo property)
	{
		Property = property;
	}

	public EntityKeyBuilder HasKeyGenerator(IEntityKeyGenerator keyGenerator)
	{
		KeyGenerator = keyGenerator;
		return this;
	}
}