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
	[Obsolete("Replace with IEntityDefinition _if_ actually needed")]
	public Type EntityType { get; }
	[Obsolete("Key is defined on IEntityDefinition")]
	public bool IsKey { get; }
	public string ElementName { get; }
	[Obsolete("This should be on a custom EntityProperty type (WalkedEntityProperty)?")]
	public string FullPath { get; }
	[Obsolete("This is accessible from PropertyInfo")]
	public Type PropertyType { get; }
	public PropertyInfo PropertyInfo { get; }

	public object GetValue(object entity);
	public void SetValue(object entity, object value);
}

public interface IEntityIndexDefinition
{
	public IReadOnlyCollection<IEntityPropertyDefinition> Properties { get; }
	[Obsolete("Index definition can point to multiple properties directly")]
	public IEntityPropertyDefinition Property { get; }
	//TODO: This will be made redundant when the broader change to support fluent comes in
	public string Path { get; }
	public string IndexName { get; }
	public bool IsUnique { get; }
	public IndexSortOrder SortOrder { get; }
	public int IndexPriority { get; }
	public IndexType IndexType { get; }
	public bool IsTenantExclusive { get; }
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
	public Type EntityType { get; set; }
	public bool IsKey { get; set; }
	public string ElementName { get; set; }
	public string FullPath { get; set; }
	public Type PropertyType { get; set; }
	public PropertyInfo PropertyInfo { get; set; }

	public object GetValue(object entity)
	{
		return PropertyInfo.GetValue(entity);
	}

	public void SetValue(object entity, object value)
	{
		PropertyInfo.SetValue(entity, value);
	}
}

public class EntityIndexDefinition : IEntityIndexDefinition
{
	public IReadOnlyCollection<IEntityPropertyDefinition> Properties { get; set; }
	public IEntityPropertyDefinition Property { get; set; }
	public string Path { get; set; }
	public string IndexName { get; set; }
	public bool IsUnique { get; set; }
	public IndexSortOrder SortOrder { get; set; }
	public int IndexPriority { get; set; }
	public IndexType IndexType { get; set; }
	public bool IsTenantExclusive { get; set; }
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