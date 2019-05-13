using System;
using System.Reflection;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Attributes
{
	public abstract class MutatePropertyAttribute : Attribute
	{
		public virtual void OnInsert(object target, IEntityProperty property) { }
		public virtual void OnUpdate(object target, IEntityProperty property) { }
		public virtual void OnSelect(object target, IEntityProperty property) { }
	}
}
