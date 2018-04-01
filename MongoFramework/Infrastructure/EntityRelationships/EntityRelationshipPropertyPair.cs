using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MongoFramework.Infrastructure.EntityRelationships
{
	public class EntityRelationshipPropertyPair
	{
		public PropertyInfo IdProperty { get; set; }
		public PropertyInfo NavigationProperty { get; set; }
		public bool IsCollection { get; set; }
		public Type CollectionEntityType { get; set; }
	}
}
