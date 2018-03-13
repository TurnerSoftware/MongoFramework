using System;
using System.Reflection;

namespace MongoFramework.Attributes
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class CreatedDateAttribute : MutatePropertyAttribute
	{
		public override void OnInsert(object target, PropertyInfo property)
		{
			if (property.PropertyType != typeof(DateTime))
			{
				throw new ArgumentException("Property is not of type DateTime");
			}

			property.SetValue(target, DateTime.UtcNow);
		}
	}
}
