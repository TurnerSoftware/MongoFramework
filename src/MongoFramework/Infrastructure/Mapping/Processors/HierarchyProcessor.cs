using MongoDB.Bson.Serialization;
using System;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class HierarchyProcessor : IMappingProcessor
	{
		public void ApplyMapping(IEntityDefinition definition, BsonClassMap classMap)
		{
			var entityType = definition.EntityType;
			if (EntityMapping.IsValidTypeToMap(entityType.BaseType) && !EntityMapping.IsRegistered(entityType.BaseType))
			{
				EntityMapping.RegisterType(entityType.BaseType);
			}
			else
			{
				classMap.SetIsRootClass(true);
			}
		}
	}
}
