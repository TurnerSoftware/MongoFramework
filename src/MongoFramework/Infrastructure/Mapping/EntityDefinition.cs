using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace MongoFramework.Infrastructure.Mapping;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed record EntityDefinition
{
	public Type EntityType { get; init; }
	public string CollectionName { get; init; }
	public KeyDefinition Key { get; init; }
	public IReadOnlyList<PropertyDefinition> Properties { get; init; } = Array.Empty<PropertyDefinition>();
	public IReadOnlyList<IndexDefinition> Indexes { get; init; } = Array.Empty<IndexDefinition>();
	public ExtraElementsDefinition ExtraElements { get; init; }

	[DebuggerNonUserCode]
	private string DebuggerDisplay => $"EntityType = {EntityType.Name}, Collection = {CollectionName}, Properties = {Properties.Count}, Indexes = {Indexes.Count}";
}

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed record PropertyDefinition
{
	public PropertyInfo PropertyInfo { get; init; }
	public string ElementName { get; init; }

	private Func<object, object> getValueDelegate;
	public object GetValue(object entity)
	{
		if (getValueDelegate is null)
		{
			// Effectively results in the following expression
			// object t => (object)(({PropertyType})t).{PropertyName}
			var parameter = Expression.Parameter(typeof(object), "t");
			var lambda = Expression.Lambda<Func<object, object>>(
				Expression.Convert(
					Expression.MakeMemberAccess(
						Expression.Convert(
							parameter,
							PropertyInfo.DeclaringType
						),
						PropertyInfo
					),
					typeof(object)
				), 
				parameter
			);
			getValueDelegate = lambda.Compile();
		}

		return getValueDelegate(entity);
	}

	public void SetValue(object entity, object value)
	{
		PropertyInfo.SetValue(entity, value);
	}

	[DebuggerNonUserCode]
	private string DebuggerDisplay => $"PropertyInfo = {PropertyInfo.Name}, ElementName = {ElementName}";
}

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed record IndexDefinition
{
	public IReadOnlyList<IndexPathDefinition> IndexPaths { get; init; }
	public string IndexName { get; init; }
	public bool IsUnique { get; init; }
	public bool IsTenantExclusive { get; init; }

	[DebuggerNonUserCode]
	private string DebuggerDisplay => $"IndexName = {IndexName}, IndexPaths = {IndexPaths.Count}, IsUnique = {IsUnique}";
}

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed record IndexPathDefinition
{
	public string Path { get; init; }
	public IndexType IndexType { get; init; }
	public IndexSortOrder SortOrder { get; init; }

	[DebuggerNonUserCode]
	private string DebuggerDisplay => $"Path = {Path}, IndexType = {IndexType}, SortOrder = {SortOrder}";
}

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed record KeyDefinition
{
	public PropertyDefinition Property { get; init; }
	public IEntityKeyGenerator KeyGenerator { get; init; }

	[DebuggerNonUserCode]
	private string DebuggerDisplay => $"PropertyInfo = {Property.PropertyInfo.Name}, ElementName = {Property.ElementName}";
}

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed record ExtraElementsDefinition
{
	public PropertyDefinition Property { get; init; }
	public bool IgnoreExtraElements { get; init; }
	public bool IgnoreInherited { get; init; }

	[DebuggerNonUserCode]
	private string DebuggerDisplay
	{
		get
		{
			if (IgnoreExtraElements)
			{
				return "IgnoreExtraElements = true";
			}
			else
			{
				return $"PropertyInfo = {Property.PropertyInfo.Name}, ElementName = {Property.ElementName}";
			}
		}
	}
}