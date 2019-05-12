using System;
using System.Reflection;

namespace MongoFramework.Infrastructure.Mapping
{
	public interface IEntityRelationship
	{
		IEntityProperty IdProperty { get; set; }
		IEntityProperty NavigationProperty { get; set; }
		Type EntityType { get; set; }
		bool IsCollection { get; set; }
	}
}
