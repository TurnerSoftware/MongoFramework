using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace MongoFramework.Infrastructure.Mapping;

public class EntityDefinitionBuilder
{
	private readonly Dictionary<PropertyInfo, EntityPropertyBuilder> propertyBuilders = new();
	private readonly List<EntityIndexBuilder> indexBuilders = new();
	private EntityKeyBuilder keyBuilder;

	public Type EntityType { get; private set; }
	public string CollectionName { get; private set; }
	public PropertyInfo ExtraElementsProperty { get; private set; }

	protected EntityDefinitionBuilder(Type entityType)
	{
		EntityType = entityType;
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
		var propertyInfo = EntityType.GetProperty(propertyName);
		if (propertyInfo is null)
		{
			throw new ArgumentException($"Property \"{propertyInfo.Name}\" can not be found on \"{EntityType.Name}\".", nameof(propertyName));
		}
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
		keyBuilder = new EntityKeyBuilder(propertyInfo);

		builder(keyBuilder);
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

	public EntityDefinitionBuilder HasIndex(IReadOnlyCollection<PropertyInfo> properties, Action<EntityIndexBuilder> builder)
	{
		//TODO: This needs to be far smarter as indexes can be complex through deeply nested types
		foreach (var propertyInfo in properties)
		{
			if (!propertyInfo.DeclaringType.IsAssignableFrom(EntityType))
			{
				throw new ArgumentException($"Property \"{propertyInfo.Name}\" is not accessible from \"{EntityType.Name}\".", nameof(properties));
			}

			if (!propertyInfo.CanWrite || !propertyInfo.CanRead)
			{
				throw new ArgumentException($"Property \"{propertyInfo.Name}\" must be both readable and writeable.", nameof(properties));
			}
		}

		var indexBuilder = new EntityIndexBuilder(properties);
		indexBuilders.Add(indexBuilder);

		builder(indexBuilder);
		return this;
	}

	public EntityDefinitionBuilder HasIndex(IEnumerable<string> propertyNames, Action<EntityIndexBuilder> builder)
	{
		var properties = new List<PropertyInfo>();
		foreach (var name in propertyNames)
		{
			var propertyInfo = EntityType.GetProperty(name);
			if (propertyInfo is null)
			{
				throw new ArgumentException($"Property \"{propertyInfo.Name}\" can not be found on \"{EntityType.Name}\" or any base types.", nameof(propertyNames));
			}
			properties.Add(propertyInfo);
		}

		return HasIndex(properties, builder);
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
}

public class EntityDefinitionBuilder<TEntity> : EntityDefinitionBuilder
{
	public EntityDefinitionBuilder() : base(typeof(TEntity)) { }

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
		//TODO: This needs to be far smarter as indexes can be complex through deeply nested types
		if (indexExpression.Body is MemberExpression memberExpression && memberExpression.Member is PropertyInfo singleIndexPropertyInfo)
		{
			var properties = new PropertyInfo[] { singleIndexPropertyInfo };
			return HasIndex(properties, builder) as EntityDefinitionBuilder<TEntity>;
		}
		else if (indexExpression.Body is NewExpression newObjExpression)
		{
			var properties = new List<PropertyInfo>();
			foreach (var expression in newObjExpression.Arguments)
			{
				if (expression is not MemberExpression argumentMemberExpression || argumentMemberExpression.Member is not PropertyInfo multiIndexPropertyInfo)
				{
					throw new ArgumentException($"Invalid expression \"{expression}\" for index. Must be a property to be bound as an index.", nameof(indexExpression));
				}
				properties.Add(multiIndexPropertyInfo);
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
}

public sealed class EntityPropertyBuilder
{
	public PropertyInfo Property { get; }
	public string ElementName { get; private set; }

	public EntityPropertyBuilder(PropertyInfo propertyInfo)
	{
		Property = propertyInfo;
	}

	public EntityPropertyBuilder HasElementName(string elementName)
	{
		ElementName = elementName;
		return this;
	}
}

public sealed class EntityIndexBuilder
{
	public IReadOnlyCollection<PropertyInfo> Properties { get; }
	public string IndexName { get; }

	public IReadOnlyCollection<IndexType> PropertyIndexTypes { get; private set; }
	public IReadOnlyCollection<bool> DescendingProperties { get; private set; }
	public bool Unique { get; private set; }

	public bool TenantExclusive { get; private set; }

	public EntityIndexBuilder(IReadOnlyCollection<PropertyInfo> properties, string indexName = null)
	{
		Properties = properties;
		IndexName = indexName;
	}

	public EntityIndexBuilder HasType(params IndexType[] indexTypes)
	{
		if (indexTypes.Length > Properties.Count)
		{
			throw new ArgumentException("Too many items in list of descending indexes", nameof(indexTypes));
		}

		PropertyIndexTypes = indexTypes;
		return this;
	}

	public EntityIndexBuilder IsDescending(params bool[] descending)
	{
		if (descending.Length > Properties.Count)
		{
			throw new ArgumentException("Too many items in list of descending indexes", nameof(descending));
		}

		DescendingProperties = descending;
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