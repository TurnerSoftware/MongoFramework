using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MongoFramework.Infrastructure.Mapping
{
	public interface IEntityPropertyMap
	{
		Type EntityType { get; }
		bool IsKey { get; }
		string ElementName { get; }
		string FullPath { get; }
		Type PropertyType { get; }
		PropertyInfo Property { get; }
	}
}
