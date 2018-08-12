using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MongoFramework.Internal
{
	public readonly struct DbSetInfo
	{
		public DbSetInfo(PropertyInfo property, Type entityType)
		{
			Property = property;
			PropertyType = property.PropertyType;
			EntityType = entityType;
		}

		public PropertyInfo Property { get; }
		public Type PropertyType { get; }
		public Type EntityType { get; }
	}
}
