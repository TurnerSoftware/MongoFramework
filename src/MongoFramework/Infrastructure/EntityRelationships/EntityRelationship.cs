using System;
using System.Reflection;

namespace MongoFramework.Infrastructure.EntityRelationships
{
	public class EntityRelationship
	{
		public PropertyInfo IdProperty { get; set; }
		public PropertyInfo NavigationProperty { get; set; }
		public Type EntityType { get; set; }
		public bool IsCollection { get; set; }
	}
}
