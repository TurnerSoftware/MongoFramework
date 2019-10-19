using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class EntityRelationshipProcessor : IMappingProcessor
	{
		private static readonly Type[] IdTypes = new[] { typeof(string), typeof(Guid), typeof(ObjectId) };
		public void ApplyMapping(IEntityDefinition definition, BsonClassMap classMap)
		{
			definition.Relationships = GetEntityRelationships(definition).ToArray();

			var removeProperties = new HashSet<string>();

			foreach (var relationship in definition.Relationships)
			{
				if (relationship.IsCollection)
				{
					var memberMap = classMap.MapMember(relationship.NavigationProperty.PropertyInfo);
					var serializerType = typeof(EntityNavigationCollectionSerializer<>).MakeGenericType(relationship.EntityType);
					var collectionSerializer = Activator.CreateInstance(serializerType, relationship.IdProperty) as IBsonSerializer;
					memberMap.SetSerializer(collectionSerializer);
				}
				else
				{
					removeProperties.Add(relationship.NavigationProperty.FullPath);
					classMap.UnmapMember(relationship.NavigationProperty.PropertyInfo);
				}
			}

			//Remove navigation properties
			definition.Properties = definition.Properties.Where(p => !removeProperties.Contains(p.FullPath)).ToArray();
		}
		private IEnumerable<IEntityRelationship> GetEntityRelationships(IEntityDefinition definition)
		{
			var entityType = definition.EntityType;
			var propertyMap = definition.GetAllProperties().ToDictionary(p => p.PropertyInfo.Name, p => p);

			foreach (var mapping in propertyMap)
			{
				var currentProperty = mapping.Value;
				var propertyInfo = currentProperty.PropertyInfo;
				var propertyType = currentProperty.PropertyType;

				//For a single entity relationship
				var foreignKeyAttr = propertyInfo.GetCustomAttribute<ForeignKeyAttribute>();
				if (foreignKeyAttr != null)
				{
					var linkedProperty = propertyMap.ContainsKey(foreignKeyAttr.Name) ? propertyMap[foreignKeyAttr.Name] : null;

					if (linkedProperty == null)
					{
						throw new InvalidOperationException($"Can't find property {foreignKeyAttr.Name} in {entityType.Name} as indicated by the {nameof(ForeignKeyAttribute)}.");
					}
					else if (IdTypes.Contains(currentProperty.PropertyType))
					{
						yield return new EntityRelationship
						{
							IdProperty = currentProperty,
							NavigationProperty = linkedProperty,
							EntityType = linkedProperty.PropertyType
						};
					}
					else if (IdTypes.Contains(linkedProperty.PropertyType))
					{
						yield return new EntityRelationship
						{
							IdProperty = linkedProperty,
							NavigationProperty = currentProperty,
							EntityType = currentProperty.PropertyType
						};
					}
					else
					{
						throw new InvalidOperationException($"Unable to determine the Id property between {currentProperty} and {linkedProperty}. Check the types for these properties are valid.");
					}

					continue;
				}

				//For an entity collection relationship
				if (propertyInfo.CanRead && propertyInfo.GetGetMethod().IsVirtual && propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
				{
					var collectionEntityType = propertyType.GetGenericArguments().FirstOrDefault();
					var inversePropertyAttr = propertyInfo.GetCustomAttribute<InversePropertyAttribute>();
					var relatedEntityMapping = EntityMapping.GetOrCreateDefinition(collectionEntityType).GetAllProperties();

					IEntityProperty idProperty;

					if (inversePropertyAttr != null)
					{
						idProperty = relatedEntityMapping.Where(p => p.PropertyInfo.Name == inversePropertyAttr.Property).FirstOrDefault();

						if (idProperty == null)
						{
							throw new InvalidOperationException($"Can't find property {inversePropertyAttr.Property} in {collectionEntityType} as indicated by the InversePropertyAttribute on {currentProperty}");
						}
						else if (!IdTypes.Contains(idProperty.PropertyType))
						{
							throw new InvalidOperationException($"The Id property {inversePropertyAttr.Property} in {collectionEntityType.Name} isn't of a compatible type.");
						}
					}
					else
					{
						//Default to the Id when no specific foreign key is found
						idProperty = relatedEntityMapping.Where(p => p.IsKey).FirstOrDefault();
					}

					yield return new EntityRelationship
					{
						IdProperty = idProperty,
						NavigationProperty = currentProperty,
						EntityType = collectionEntityType,
						IsCollection = true
					};
				}
			}
		}
	}
}
