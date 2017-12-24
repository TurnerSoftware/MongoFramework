using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace MongoFramework.Infrastructure.Mapping
{
	public interface IMappingProcessor
	{
		void ApplyMapping(Type entityType, BsonClassMap classMap);
	}
}
