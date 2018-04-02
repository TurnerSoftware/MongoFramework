using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson.Serialization;
using System.Reflection;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using MongoFramework.Infrastructure.EntityRelationships;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class NavigationPropertyProcessor : IMappingProcessor
	{
		public void ApplyMapping(Type entityType, BsonClassMap classMap)
		{
			var relationships = EntityRelationshipHelper.GetRelationshipsForType(entityType);

			foreach (var relationship in relationships)
			{
				if (relationship.IsCollection)
				{
					var memberMap = classMap.MapMember(relationship.NavigationProperty);
					var serializerType = typeof(EntityNavigationCollectionSerializer<>).MakeGenericType(relationship.CollectionEntityType);
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
