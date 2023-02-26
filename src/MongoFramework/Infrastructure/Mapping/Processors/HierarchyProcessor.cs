namespace MongoFramework.Infrastructure.Mapping.Processors;

public class HierarchyProcessor : IMappingProcessor
{
	public void ApplyMapping(EntityDefinitionBuilder definitionBuilder)
	{
		var baseType = definitionBuilder.EntityType.BaseType;
		if (EntityMapping.IsValidTypeToMap(baseType))
		{
			definitionBuilder.MappingBuilder.Entity(baseType);
		}
	}
}
