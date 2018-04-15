using MongoDB.Driver;
using MongoFramework.Infrastructure.EntityRelationships;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Linq;
using System;
using System.Linq;
using System.Reflection;

namespace MongoFramework.Infrastructure.Mutation.Mutators
{
	public class NavigationPropertyMutator<TEntity> : IEntityMutator<TEntity>
	{
		public void MutateEntity(TEntity entity, MutatorType mutationType, IEntityMapper entityMapper, IMongoDatabase database)
		{
			var relationships = entityMapper.GetEntityRelationships();
			foreach (var relationship in relationships)
			{
				if (mutationType == MutatorType.Select)
				{
					InitialiseEntityRelationship(entity, relationship, database);
				}
				else if (mutationType == MutatorType.Create && relationship.IsCollection)
				{
					var navigationCollectionType = typeof(EntityNavigationCollection<>).MakeGenericType(relationship.EntityType);
					var navigationCollection = Activator.CreateInstance(navigationCollectionType, relationship.IdProperty.Name) as IEntityNavigationCollection;
					navigationCollection.Connect(database);
					relationship.NavigationProperty.SetValue(entity, navigationCollection);
				}
			}
		}

		private void InitialiseEntityRelationship(TEntity targetEntity, EntityRelationship relationship, IMongoDatabase database)
		{
			if (relationship.IsCollection)
			{
				var collection = relationship.NavigationProperty.GetValue(targetEntity) as IEntityNavigationCollection;
				collection.Connect(database);
			}
			else
			{
				var initialiseSingleEntityMethod = GetType().GetMethod("InitialiseSingleEntityRelationship", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(relationship.EntityType);
				initialiseSingleEntityMethod.Invoke(targetEntity, new object[] { targetEntity, relationship, database });
			}
		}

#pragma warning disable CRR0026 // Unused member - called through Reflection
		private static void InitialiseSingleEntityRelationship<TRelatedEntity>(TEntity targetEntity, EntityRelationship relationship, IMongoDatabase database)
		{
			var dbEntityReader = new DbEntityReader<TRelatedEntity>(database);
			var relationshipIdValue = relationship.IdProperty.GetValue(targetEntity);
			var loadedEntity = dbEntityReader.AsQueryable().WhereIdMatches(new[] { relationshipIdValue }).FirstOrDefault();
			relationship.NavigationProperty.SetValue(targetEntity, loadedEntity);
		}
#pragma warning restore CRR0026 // Unused member - called through Reflection
	}
}
