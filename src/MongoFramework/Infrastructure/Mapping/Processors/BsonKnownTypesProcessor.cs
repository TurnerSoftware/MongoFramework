using System.Reflection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class BsonKnownTypesProcessor : IMappingProcessor
	{
		public void ApplyMapping(IEntityDefinition definition)
		{
			var entityType = definition.EntityType;
			var bsonKnownTypesAttribute = entityType.GetCustomAttribute<BsonKnownTypesAttribute>();
			if (bsonKnownTypesAttribute != null)
			{
				foreach (var type in bsonKnownTypesAttribute.KnownTypes)
				{
					EntityMapping.TryRegisterType(type, out _);
				}
			}
		}
	}
}
