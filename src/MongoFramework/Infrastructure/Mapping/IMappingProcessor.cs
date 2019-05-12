using MongoDB.Bson.Serialization;
using System;

namespace MongoFramework.Infrastructure.Mapping
{
	public interface IMappingProcessor
	{
		void ApplyMapping(IEntityDefinition definition, BsonClassMap classMap);
	}
}
