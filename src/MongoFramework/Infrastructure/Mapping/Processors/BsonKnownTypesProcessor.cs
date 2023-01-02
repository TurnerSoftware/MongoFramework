using System.Reflection;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoFramework.Infrastructure.Mapping.Processors;

public class BsonKnownTypesProcessor : IMappingProcessor
{
	public void ApplyMapping(EntityDefinitionBuilder definitionBuilder)
	{
		var entityType = definitionBuilder.EntityType;
		var bsonKnownTypesAttribute = entityType.GetCustomAttribute<BsonKnownTypesAttribute>();
		if (bsonKnownTypesAttribute != null)
		{
			foreach (var type in bsonKnownTypesAttribute.KnownTypes)
			{
				definitionBuilder.MappingBuilder.Entity(type);
			}
		}
	}
}
