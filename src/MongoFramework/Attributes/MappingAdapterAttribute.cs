using System;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Attributes;

/// <summary>
/// Applies the specific <see cref="IMappingProcessor"/> on the entity.
/// Runs after attribute processing, so the adapter can override attributes.
/// Adapter type must have a parameterless constructor.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class MappingAdapterAttribute : Attribute
{
	/// <summary>
	/// Gets the adapter type for the attached class
	/// </summary>
	public Type MappingAdapter { get; }

	public MappingAdapterAttribute(Type adapterType)
	{
		MappingAdapter = adapterType;
	}
}
