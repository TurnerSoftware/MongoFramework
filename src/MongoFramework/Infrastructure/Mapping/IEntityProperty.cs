using System;
using System.Reflection;

namespace MongoFramework.Infrastructure.Mapping
{
	public interface IEntityProperty
	{
		Type EntityType { get; }
		bool IsKey { get; }
		string ElementName { get; }
		string FullPath { get; }
		Type PropertyType { get; }
		PropertyInfo PropertyInfo { get; }

		object GetValue(object entity);
		void SetValue(object entity, object value);
	}
}
