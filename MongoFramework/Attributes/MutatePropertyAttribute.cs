using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Attributes
{
	public abstract class MutatePropertyAttribute : Attribute
	{
		public virtual void OnInsert(object target, PropertyInfo property) { }
		public virtual void OnUpdate(object target, PropertyInfo property) { }
		public virtual void OnSelect(object target, PropertyInfo property) { }
	}
}
