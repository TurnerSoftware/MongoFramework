using System;
using System.Reflection;

namespace MongoFramework.Infrastructure.Mapping
{
	public class EntityRelationship : IEntityRelationship
	{
		public IEntityProperty IdProperty { get; set; }
		public IEntityProperty NavigationProperty { get; set; }
		public Type EntityType { get; set; }
		public bool IsCollection { get; set; }
	}
}
