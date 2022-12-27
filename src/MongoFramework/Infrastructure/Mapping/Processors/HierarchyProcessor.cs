using MongoDB.Bson.Serialization;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class HierarchyProcessor : IMappingProcessor
	{
		public void ApplyMapping(IEntityDefinition definition)
		{
			var entityType = definition.EntityType;
			if (EntityMapping.IsValidTypeToMap(entityType.BaseType))
			{
				EntityMapping.TryRegisterType(entityType.BaseType, out _);
			}
		}
	}
}
