using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MongoFramework.Infrastructure.Mapping;

public interface IEntityDefinition
{
	public Type EntityType { get; set; }
	public string CollectionName { get; set; }
	public IEntityKeyDefinition Key { get; set; }
	public IEnumerable<IEntityPropertyDefinition> Properties { get; set; }
	public IEnumerable<IEntityIndexDefinition> Indexes { get; set; }
	public IEntityExtraElementsDefinition ExtraElements { get; set; }
}

public interface IEntityPropertyDefinition
{
	public PropertyInfo PropertyInfo { get; }
	public string ElementName { get; }

	public object GetValue(object entity);
	public void SetValue(object entity, object value);
}

public interface IEntityIndexDefinition
{
	public IReadOnlyList<IEntityIndexPathDefinition> IndexPaths { get; }
	public string IndexName { get; }
	public bool IsUnique { get; }
	public bool IsTenantExclusive { get; }
}

public interface IEntityIndexPathDefinition
{
	public string Path { get; }
	public IndexType IndexType { get; }
	public IndexSortOrder SortOrder { get; }
}

public interface IEntityExtraElementsDefinition
{
	public IEntityPropertyDefinition Property { get; }
	public bool IgnoreExtraElements { get; }
	public bool IgnoreInherited { get; }
}

public interface IEntityKeyDefinition
{
	public IEntityPropertyDefinition Property { get; }
	public IEntityKeyGenerator KeyGenerator { get; }
}

public class EntityDefinition : IEntityDefinition
{
	public Type EntityType { get; set; }
	public string CollectionName { get; set; }
	public IEntityKeyDefinition Key { get; set; }
	public IEnumerable<IEntityPropertyDefinition> Properties { get; set; } = Enumerable.Empty<IEntityPropertyDefinition>();
	public IEnumerable<IEntityIndexDefinition> Indexes { get; set; } = Enumerable.Empty<IEntityIndexDefinition>();
	public IEntityExtraElementsDefinition ExtraElements { get; set; }
}

public class EntityPropertyDefinition : IEntityPropertyDefinition
{
	public PropertyInfo PropertyInfo { get; set; }
	public string ElementName { get; set; }

	public object GetValue(object entity)
	{
		return PropertyInfo.GetValue(entity);
	}

	public void SetValue(object entity, object value)
	{
		PropertyInfo.SetValue(entity, value);
	}
}

public sealed record EntityIndexDefinition : IEntityIndexDefinition
{
	public IReadOnlyList<IEntityIndexPathDefinition> IndexPaths { get; init; }
	public string IndexName { get; init; }
	public bool IsUnique { get; init; }
	public bool IsTenantExclusive { get; init; }
}

public sealed record EntityIndexPathDefinition : IEntityIndexPathDefinition
{
	public string Path { get; init; }
	public IndexType IndexType { get; init; }
	public IndexSortOrder SortOrder { get; init; }
}

public sealed record EntityKeyDefinition : IEntityKeyDefinition
{
	public IEntityPropertyDefinition Property { get; init; }
	public IEntityKeyGenerator KeyGenerator { get; init; }
}

public sealed record EntityExtraElementsDefinition : IEntityExtraElementsDefinition
{
	public IEntityPropertyDefinition Property { get; init; }
	public bool IgnoreExtraElements { get; init; }
	public bool IgnoreInherited { get; init; }
}