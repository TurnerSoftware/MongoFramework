using MongoDB.Bson.Serialization;
using System;

namespace MongoFramework.Infrastructure.Mapping
{
	public interface IMappingProcessor
	{
		void ApplyMapping(Type entityType, BsonClassMap classMap);
	}
}
