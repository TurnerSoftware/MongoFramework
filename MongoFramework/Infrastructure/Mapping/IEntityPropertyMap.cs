using System;
using System.Reflection;

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
