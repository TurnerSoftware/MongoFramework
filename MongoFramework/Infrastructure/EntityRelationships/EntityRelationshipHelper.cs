using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Linq;

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
					var linkedProperty = propertyMap.ContainsKey(foreignKeyAttr.Name) ? propertyMap[foreignKeyAttr.Name] : null;

					if (linkedProperty == null)
					{
						throw new MongoFrameworkMappingException($"Can't find property {foreignKeyAttr.Name} on {entityType.Name} as indicated by the ForeignKeyAttribute.");
					}
					else if (IdTypes.Contains(currentProperty.PropertyType))
					{
						yield return new EntityRelationshipPropertyPair
						{
							IdProperty = currentProperty,
							NavigationProperty = linkedProperty,
							EntityType = linkedProperty.PropertyType
						};
					}
					else if (IdTypes.Contains(linkedProperty.PropertyType))
					{
						yield return new EntityRelationshipPropertyPair
						{
							IdProperty = linkedProperty,
							NavigationProperty = currentProperty,
							EntityType = currentProperty.PropertyType
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
						throw new NotImplementedException("InversePropertyAttribute not supported");
						//While the logic below is correct, the EntityNavigationCollection doesn't support loading entities by arbitary properties
						//Really what needs to change is a method like "WhereIdMatches" but takes in the property to look at
						//This means `BeginImport` needs to know the relationship details
						//This means `NavigationPropertyProcessor` needs to pass it to the serializer on creation
						//All doable but not right now...

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
						EntityType = collectionEntityType,
						IsCollection = true
					};
				}
			}
		}

		public static bool CheckForNavigationPropertyChanges<TEntity>(TEntity entity)
		{
			var relationships = GetRelationshipsForType(typeof(TEntity));

			foreach (var relationship in relationships)
			{
				if (relationship.IsCollection)
				{
					var checkForNavigationCollectionPropertyChangesMethod = typeof(EntityRelationshipHelper).GetMethod("CheckForNavigationCollectionPropertyChanges", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(relationship.EntityType);
					var hasChanges = (bool)checkForNavigationCollectionPropertyChangesMethod.Invoke(entity, new object[] { entity, relationship });
					if (hasChanges)
					{
						return true;
					}
				}
				else
				{
					//Can't detect if changes exist on non-collection relationships
					return true;
				}
			}

			return false;
		}

#pragma warning disable CRR0026 // Unused member - called through Reflection
		private static bool CheckForNavigationCollectionPropertyChanges<TEntity>(object targetEntity, EntityRelationshipPropertyPair relationship)
		{
			var navigationCollection = relationship.NavigationProperty.GetValue(targetEntity) as EntityNavigationCollection<TEntity>;
			if (navigationCollection != null)
			{
				if (navigationCollection.GetEntries().Any(e => e.HasChanges()))
				{
					return true;
				}
			}
			return false;
		}
#pragma warning restore CRR0026 // Unused member - called through Reflection

		public static void InitialiseNavigationProperty(object targetEntity, EntityRelationshipPropertyPair relationship)
		{
			if (relationship.IsCollection)
			{
				var navigationPropertyType = typeof(EntityNavigationCollection<>).MakeGenericType(relationship.EntityType);
				var navigationProperty = Activator.CreateInstance(navigationPropertyType);
				relationship.NavigationProperty.SetValue(targetEntity, navigationProperty);
			}
		}

		public static void LoadNavigationProperty(object targetEntity, EntityRelationshipPropertyPair relationship, IMongoDatabase database)
		{
			if (relationship.IsCollection)
			{
				var collection = relationship.NavigationProperty.GetValue(targetEntity) as IEntityNavigationCollection;
				collection.FinaliseImport(database);
			}
			else
			{
				var loadSingleEntityMethod = typeof(EntityRelationshipHelper).GetMethod("LoadSingleEntityProperty", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(relationship.EntityType);
				loadSingleEntityMethod.Invoke(targetEntity, new[] { targetEntity, relationship, database });
			}
		}

#pragma warning disable CRR0026 // Unused member - called through Reflection
		private static void LoadSingleEntityProperty<TEntity>(object targetEntity, EntityRelationshipPropertyPair relationship, IMongoDatabase database)
		{
			var dbEntityReader = new DbEntityReader<TEntity>(database);
			var relationshipIdValue = relationship.IdProperty.GetValue(targetEntity);
			var loadedEntity = dbEntityReader.AsQueryable().WhereIdMatches(new[] { relationshipIdValue }).FirstOrDefault();
			relationship.NavigationProperty.SetValue(targetEntity, loadedEntity);
		}
#pragma warning restore CRR0026 // Unused member - called through Reflection

		public static void SaveNavigationProperty(object targetEntity, EntityRelationshipPropertyPair relationship, IMongoDatabase database)
		{
			if (relationship.IsCollection)
			{
				var collection = relationship.NavigationProperty.GetValue(targetEntity) as IEntityNavigationCollection;
				collection?.WriteChanges(database);
			}
			else
			{
				var saveSingleEntityMethod = typeof(EntityRelationshipHelper).GetMethod("SaveSingleEntityProperty", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(relationship.EntityType);
				saveSingleEntityMethod.Invoke(targetEntity, new[] { targetEntity, relationship, database });
			}
		}

#pragma warning disable CRR0026 // Unused member - called through Reflection
		private static void SaveSingleEntityProperty<TEntity>(object targetEntity, EntityRelationshipPropertyPair relationship, IMongoDatabase database)
		{
			var dbEntityWriter = new DbEntityWriter<TEntity>(database);
			var navigationEntity = (TEntity)relationship.NavigationProperty.GetValue(targetEntity);

			if (navigationEntity == null)
			{
				return;
			}

			var collection = new DbEntityCollection<TEntity>();
			var entityState = dbEntityWriter.EntityMapper.GetIdValue(navigationEntity) == null ? DbEntityEntryState.Added : DbEntityEntryState.Updated;
			collection.Update(navigationEntity, entityState);

			dbEntityWriter.Write(collection);

			var idValue = dbEntityWriter.EntityMapper.GetIdValue(navigationEntity);
			relationship.IdProperty.SetValue(targetEntity, idValue);
		}
#pragma warning restore CRR0026 // Unused member - called through Reflection
	}
}
