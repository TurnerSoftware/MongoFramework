using System;
using MongoDB.Bson.Serialization;

namespace MongoFramework.Infrastructure.Mapping
{
	public interface IMappingProcessor
	{
		void ApplyMapping(Type entityType, BsonClassMap classMap);
	}
}
