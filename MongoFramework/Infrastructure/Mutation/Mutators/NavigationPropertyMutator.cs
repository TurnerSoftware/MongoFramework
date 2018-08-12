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
		public void MutateEntity(TEntity entity, MutatorType mutationType, IDbContextSettings settings)
		{
			var relationships = settings.GetEntityMapper<TEntity>().GetEntityRelationships();
			foreach (var relationship in relationships)
			{
				if (mutationType == MutatorType.Select)
				{
					InitialiseEntityRelationship(entity, relationship, settings);
				}
				else if (mutationType == MutatorType.Create && relationship.IsCollection)
				{
					var navigationCollectionType = typeof(EntityNavigationCollection<>).MakeGenericType(relationship.EntityType);
					var navigationCollection = Activator.CreateInstance(navigationCollectionType, relationship.IdProperty.Name) as IEntityNavigationCollection;
					navigationCollection.Connect(settings);
					relationship.NavigationProperty.SetValue(entity, navigationCollection);
				}
			}
		}

		private void InitialiseEntityRelationship(TEntity targetEntity, EntityRelationship relationship, IDbContextSettings settings)
		{
			if (relationship.IsCollection)
			{
				var collection = relationship.NavigationProperty.GetValue(targetEntity) as IEntityNavigationCollection;
				collection.Connect(settings);
			}
			else
			{
				var initialiseSingleEntityMethod = GetType().GetMethod("InitialiseSingleEntityRelationship", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(relationship.EntityType);
				initialiseSingleEntityMethod.Invoke(targetEntity, new object[] { targetEntity, relationship, settings });
			}
		}

#pragma warning disable CRR0026 // Unused member - called through Reflection
		private static void InitialiseSingleEntityRelationship<TRelatedEntity>(TEntity targetEntity, EntityRelationship relationship, IDbContextSettings settings)
		{
			var dbEntityReader = new DbEntityReader<TRelatedEntity>(settings);
			var relationshipIdValue = relationship.IdProperty.GetValue(targetEntity);
			var loadedEntity = dbEntityReader.AsQueryable().WhereIdMatches(new[] { relationshipIdValue }).FirstOrDefault();
			relationship.NavigationProperty.SetValue(targetEntity, loadedEntity);
		}
#pragma warning restore CRR0026 // Unused member - called through Reflection
	}
}
