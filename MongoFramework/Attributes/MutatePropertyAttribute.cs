using System;
using System.Reflection;

namespace MongoFramework.Attributes
{
	public abstract class MutatePropertyAttribute : Attribute
	{
		public virtual void OnInsert(object target, PropertyInfo property) { }
		public virtual void OnUpdate(object target, PropertyInfo property) { }
		public virtual void OnSelect(object target, PropertyInfo property) { }
	}
}
