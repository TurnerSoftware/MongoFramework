using System;
using System.Collections.Generic;
using System.Reflection;

namespace MongoFramework.Infrastructure.Mapping;

public sealed record EntityDefinition
{
	public Type EntityType { get; init; }
	public string CollectionName { get; init; }
	public KeyDefinition Key { get; init; }
	public IReadOnlyList<PropertyDefinition> Properties { get; init; } = Array.Empty<PropertyDefinition>();
	public IReadOnlyList<IndexDefinition> Indexes { get; init; } = Array.Empty<IndexDefinition>();
	public ExtraElementsDefinition ExtraElements { get; init; }
}

public sealed record PropertyDefinition
{
	public PropertyInfo PropertyInfo { get; init; }
	public string ElementName { get; init; }

	public object GetValue(object entity)
	{
		return PropertyInfo.GetValue(entity);
	}

	public void SetValue(object entity, object value)
	{
		PropertyInfo.SetValue(entity, value);
	}
}

public sealed record IndexDefinition
{
	public IReadOnlyList<IndexPathDefinition> IndexPaths { get; init; }
	public string IndexName { get; init; }
	public bool IsUnique { get; init; }
	public bool IsTenantExclusive { get; init; }
}

public sealed record IndexPathDefinition
{
	public string Path { get; init; }
	public IndexType IndexType { get; init; }
	public IndexSortOrder SortOrder { get; init; }
}

public sealed record KeyDefinition
{
	public PropertyDefinition Property { get; init; }
	public IEntityKeyGenerator KeyGenerator { get; init; }
}

public sealed record ExtraElementsDefinition
{
	public PropertyDefinition Property { get; init; }
	public bool IgnoreExtraElements { get; init; }
	public bool IgnoreInherited { get; init; }
}