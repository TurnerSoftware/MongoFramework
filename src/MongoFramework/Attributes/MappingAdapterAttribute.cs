using System;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Attributes
{
	/// <summary>
	/// Allows an IMappingProcessor to override definitions in code.  Runs after attribute processing, so the adapter can override attributes.
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
			if (!typeof(IMappingProcessor).IsAssignableFrom(adapterType))
			{
				throw new ArgumentException("Mapping Adapter Type must implement IMappingProcessor", nameof(adapterType));
			}

			MappingAdapter = adapterType;
		}
	}
}
