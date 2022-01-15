using MongoDB.Bson.Serialization;

namespace MongoFramework.Infrastructure.Mapping
{
	public interface IMappingProcessor
	{
		void ApplyMapping(IEntityDefinition definition, BsonClassMap classMap);
	}
}
