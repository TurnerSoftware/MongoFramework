using MongoDB.Bson.Serialization;
using System;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class HierarchyProcessor : IMappingProcessor
	{
		public void ApplyMapping(Type entityType, BsonClassMap classMap, IMongoDbConnection connection)
		{
			if (entityType != typeof(object) && entityType.BaseType != typeof(object))
			{
				connection.GetEntityMapper(entityType.BaseType);
			}
			else
			{
				classMap.SetIsRootClass(true);
			}
		}
	}
}
