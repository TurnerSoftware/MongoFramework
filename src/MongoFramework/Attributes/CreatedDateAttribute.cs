using System;
using System.Reflection;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class CreatedDateAttribute : MutatePropertyAttribute
	{
		public override void OnInsert(object target, IEntityProperty property)
		{
			if (property.PropertyType != typeof(DateTime))
			{
				throw new ArgumentException("Property is not of type DateTime");
			}

			property.SetValue(target, DateTime.UtcNow);
		}
	}
}
