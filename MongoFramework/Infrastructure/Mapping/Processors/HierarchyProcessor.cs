using System;
using MongoDB.Bson.Serialization;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class HierarchyProcessor : IMappingProcessor
	{
		public void ApplyMapping(Type entityType, BsonClassMap classMap)
		{
			if (entityType.BaseType != typeof(object))
			{
				new EntityMapper(entityType.BaseType);
			}
			else
			{
				classMap.SetIsRootClass(true);
			}
		}
	}
}
