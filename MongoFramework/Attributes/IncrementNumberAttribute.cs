using System;
using System.Reflection;

namespace MongoFramework.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class IncrementNumberAttribute : MutatePropertyAttribute
	{
		public int IncrementAmount { get; private set; }
		public bool OnUpdateOnly { get; private set; }

		public IncrementNumberAttribute() : this(false) { }
		public IncrementNumberAttribute(bool onUpdateOnly) : this(1, onUpdateOnly) { }

		public IncrementNumberAttribute(int incrementAmount, bool onUpdateOnly = false)
		{
			IncrementAmount = incrementAmount;
			OnUpdateOnly = onUpdateOnly;
		}

		public override void OnInsert(object target, PropertyInfo property)
		{
			if (OnUpdateOnly)
			{
				return;
			}

			IncrementTarget(target, property);
		}

		public override void OnUpdate(object target, PropertyInfo property)
		{
			IncrementTarget(target, property);
		}

		private void IncrementTarget(object target, PropertyInfo property)
		{
			if (property.PropertyType != typeof(int))
			{
				throw new ArgumentException("Property is not of type Int");
			}

			var currentValue = (int)property.GetValue(target);
			currentValue += IncrementAmount;
			property.SetValue(target, currentValue);
		}
	}
}
