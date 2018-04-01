using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using MongoDB.Bson;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Infrastructure.EntityRelationships
{
	public static class EntityRelationshipHelper
	{
		public static readonly Type[] IdTypes = new[] { typeof(string), typeof(Guid), typeof(ObjectId) };
		
		public static IEnumerable<EntityRelationshipPropertyPair> GetRelationshipsForType(Type entityType)
		{
			var propertyMap = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToDictionary(p => p.Name);

			foreach (var mapping in propertyMap)
			{
				var currentProperty = mapping.Value;

				//For a single entity relationship
				var foreignKeyAttr = currentProperty.GetCustomAttribute<ForeignKeyAttribute>();
				if (foreignKeyAttr != null)
				{
					var linkedProperty = propertyMap[foreignKeyAttr.Name];

					if (linkedProperty == null)
					{
						throw new MongoFrameworkMappingException($"Can't find property {foreignKeyAttr.Name} on {entityType.Name} as indicated by the ForeignKeyAttribute.");
					}
					else if (IdTypes.Contains(currentProperty.PropertyType))
					{
						yield return new EntityRelationshipPropertyPair
						{
							IdProperty = currentProperty,
							NavigationProperty = linkedProperty
						};
					}
					else if (IdTypes.Contains(linkedProperty.PropertyType))
					{
						yield return new EntityRelationshipPropertyPair
						{
							IdProperty = linkedProperty,
							NavigationProperty = currentProperty
						};
					}
					else
					{
						throw new MongoFrameworkMappingException($"Unable to determine the Id property between {currentProperty.Name} and {linkedProperty.Name}. Check that the types for these properties is correct.");
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
							throw new MongoFrameworkMappingException($"Can't find property {inversePropertyAttr.Property} on {collectionEntityType.Name} as indicated by the InversePropertyAttribute on {currentProperty.Name} in {entityType.Name}");
						}
						else if (!IdTypes.Contains(idProperty.PropertyType))
						{
							throw new MongoFrameworkMappingException($"For the navigation property {currentProperty.Name}, the Id property {inversePropertyAttr.Property} on {collectionEntityType.Name} isn't of a compatible type.");
						}
					}
					else
					{
						idProperty = relatedEntityMapping.Where(m => m.IsKey).Select(m => m.Property).FirstOrDefault();
					}

					yield return new EntityRelationshipPropertyPair
					{
						IdProperty = idProperty,
						NavigationProperty = currentProperty,
						IsCollection = true,
						CollectionEntityType = collectionEntityType
					};
				}
			}
		}
	}
}
