using System;
using System.Reflection;

namespace MongoFramework.Infrastructure.Mapping
{
	public class EntityProperty : IEntityProperty
	{
		public Type EntityType { get; set; }
		public bool IsKey { get; set; }
		public string ElementName { get; set; }
		public string FullPath { get; set; }
		public Type PropertyType { get; set; }
		public PropertyInfo PropertyInfo { get; set; }

		public object GetValue(object entity)
		{
			return PropertyInfo.GetValue(entity);
		}

		public void SetValue(object entity, object value)
		{
			PropertyInfo.SetValue(entity, value);
		}
	}
}
