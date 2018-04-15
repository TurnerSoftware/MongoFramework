using MongoDB.Bson;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace MongoFramework.Infrastructure.EntityRelationships
{
	public static class EntityMapperExtensions
	{
		public static readonly Type[] IdTypes = new[] { typeof(string), typeof(Guid), typeof(ObjectId) };

		public static IEnumerable<EntityRelationship> GetEntityRelationships(this IEntityMapper entityMapper)
		{
			var entityType = entityMapper.EntityType;
			var propertyMap = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToDictionary(p => p.Name);

			foreach (var mapping in propertyMap)
			{
				var currentProperty = mapping.Value;

				//For a single entity relationship
				var foreignKeyAttr = currentProperty.GetCustomAttribute<ForeignKeyAttribute>();
				if (foreignKeyAttr != null)
				{
					var linkedProperty = propertyMap.ContainsKey(foreignKeyAttr.Name) ? propertyMap[foreignKeyAttr.Name] : null;

					if (linkedProperty == null)
					{
						throw new MongoFrameworkMappingException($"Can't find property {foreignKeyAttr.Name} in {entityType.Name} as indicated by the ForeignKeyAttribute.");
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
						throw new MongoFrameworkMappingException($"Unable to determine the Id property between {currentProperty.Name} and {linkedProperty.Name}. Check the types for these properties are correct.");
					}

					continue;
				}

				//For an entity collection relationship
				var propertyType = currentProperty.PropertyType;
				if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
				{
					var collectionEntityType = propertyType.GetGenericArguments().FirstOrDefault();
					var inversePropertyAttr = currentProperty.GetCustomAttribute<InversePropertyAttribute>();
					var relatedEntityMapping = new EntityMapper(collectionEntityType).GetEntityMapping();

					PropertyInfo idProperty = null;

					if (inversePropertyAttr != null)
					{
						idProperty = relatedEntityMapping.Where(m => m.Property.Name == inversePropertyAttr.Property).Select(m => m.Property).FirstOrDefault();

						if (idProperty == null)
						{
							throw new MongoFrameworkMappingException($"Can't find property {inversePropertyAttr.Property} in {collectionEntityType} as indicated by the InversePropertyAttribute on {currentProperty.Name} in {entityType}");
						}
						else if (!IdTypes.Contains(idProperty.PropertyType))
						{
							throw new MongoFrameworkMappingException($"The Id property {inversePropertyAttr.Property} in {collectionEntityType.Name} isn't of a compatible type.");
						}
					}
					else
					{
						//Default to the Id when no specific foreign key is found
						idProperty = relatedEntityMapping.Where(m => m.IsKey).Select(m => m.Property).FirstOrDefault();
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
