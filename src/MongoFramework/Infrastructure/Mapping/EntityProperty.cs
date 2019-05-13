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

		public bool Equals(IEntityProperty other)
		{
			if (other == null)
			{
				return false;
			}

			return EntityType == other.EntityType &&
				IsKey == other.IsKey &&
				ElementName == other.ElementName &&
				FullPath == other.FullPath &&
				PropertyType == other.PropertyType &&
				PropertyInfo == other.PropertyInfo;
		}

		public object GetValue(object entity)
		{
			return PropertyInfo.GetValue(entity);
		}

		public void SetValue(object entity, object value)
		{
			PropertyInfo.SetValue(entity, value);
		}

		public override string ToString()
		{
			return $"{EntityType.Name}.{PropertyInfo.Name}";
		}
	}
}
