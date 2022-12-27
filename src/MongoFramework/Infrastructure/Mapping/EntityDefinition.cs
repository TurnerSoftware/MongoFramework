using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MongoFramework.Infrastructure.Mapping;

public interface IEntityDefinition
{
	Type EntityType { get; set; }
	string CollectionName { get; set; }
	IEntityKeyGenerator KeyGenerator { get; set; }
	IEnumerable<IEntityProperty> Properties { get; set; }
	IEnumerable<IEntityIndex> Indexes { get; set; }
}

public interface IEntityProperty
{
	Type EntityType { get; }
	bool IsKey { get; }
	string ElementName { get; }
	string FullPath { get; }
	Type PropertyType { get; }
	PropertyInfo PropertyInfo { get; }

	object GetValue(object entity);
	void SetValue(object entity, object value);
}

public interface IEntityIndex
{
	IEntityProperty Property { get; }
	string IndexName { get; }
	bool IsUnique { get; }
	IndexSortOrder SortOrder { get; }
	int IndexPriority { get; }
	IndexType IndexType { get; }
	bool IsTenantExclusive { get; set; }
}

public class EntityDefinition : IEntityDefinition
{
	public Type EntityType { get; set; }
	public string CollectionName { get; set; }
	public IEntityKeyGenerator KeyGenerator { get; set; }
	public IEnumerable<IEntityProperty> Properties { get; set; } = Enumerable.Empty<IEntityProperty>();
	public IEnumerable<IEntityIndex> Indexes { get; set; } = Enumerable.Empty<IEntityIndex>();
}

public class EntityProperty : IEntityProperty
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

public class EntityIndex : IEntityIndex
{
	public IEntityProperty Property { get; set; }
	public string IndexName { get; set; }
	public bool IsUnique { get; set; }
	public IndexSortOrder SortOrder { get; set; }
	public int IndexPriority { get; set; }
	public IndexType IndexType { get; set; }
	public bool IsTenantExclusive { get; set; }
}