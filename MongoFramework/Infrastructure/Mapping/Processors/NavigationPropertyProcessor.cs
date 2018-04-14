using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure.EntityRelationships;
using System;
using System.Linq;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class NavigationPropertyProcessor : IMappingProcessor
	{
		public void ApplyMapping(Type entityType, BsonClassMap classMap)
		{
			var entityMapper = new EntityMapper(entityType);
			var relationships = entityMapper.GetEntityRelationships();

			foreach (var relationship in relationships)
			{
				if (relationship.IsCollection)
				{
					var memberMap = classMap.MapMember(relationship.NavigationProperty);
					var serializerType = typeof(EntityNavigationCollectionSerializer<>).MakeGenericType(relationship.EntityType);
					var entitySpecificSerializer = Activator.CreateInstance(serializerType) as IBsonSerializer;
					memberMap.SetSerializer(entitySpecificSerializer);
				}
				else
				{
					classMap.UnmapMember(relationship.NavigationProperty);
				}
			}
		}
	}
}
