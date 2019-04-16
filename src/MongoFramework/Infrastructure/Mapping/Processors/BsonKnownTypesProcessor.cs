using System;
using System.Reflection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class BsonKnownTypesProcessor : IMappingProcessor
	{
		public void ApplyMapping(Type entityType, BsonClassMap classMap, IMongoDbConnection connection)
		{
			var bsonKnownTypesAttribute = entityType.GetCustomAttribute<BsonKnownTypesAttribute>();
			if (bsonKnownTypesAttribute != null)
			{
				foreach (var type in bsonKnownTypesAttribute.KnownTypes)
				{
					connection.GetEntityMapper(type);
				}
			}
		}
	}
}
