namespace MongoFramework.Infrastructure.Mapping;

public interface IMappingProcessor
{
	void ApplyMapping(EntityDefinitionBuilder definitionBuilder);
}
