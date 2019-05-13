using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Linq;
using System;
using System.Linq;
using System.Reflection;

namespace MongoFramework.Infrastructure.Mutation.Mutators
{
	public class NavigationPropertyMutator<TEntity> : IEntityMutator<TEntity> where TEntity : class
	{
		public void MutateEntity(TEntity entity, MutatorType mutationType, IMongoDbConnection connection)
		{
			var relationships = EntityMapping.GetOrCreateDefinition(typeof(TEntity)).Relationships;
			foreach (var relationship in relationships)
			{
				if (mutationType == MutatorType.Select && !relationship.IsCollection)
				{
					var initialiseSingleEntityMethod = GetType().GetMethod("InitialiseSingleEntityRelationship", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(relationship.EntityType);
					initialiseSingleEntityMethod.Invoke(entity, new object[] { entity, relationship, connection });
				}
				else if (mutationType == MutatorType.Create && relationship.IsCollection)
				{
					var navigationCollectionType = typeof(EntityNavigationCollection<>).MakeGenericType(relationship.EntityType);
					var navigationCollection = Activator.CreateInstance(navigationCollectionType, relationship.IdProperty) as IEntityNavigationCollectionBase;
					navigationCollection.SetConnection(connection);
					relationship.NavigationProperty.SetValue(entity, navigationCollection);
				}
				else if (mutationType == MutatorType.Select && relationship.IsCollection)
				{
					if (relationship.NavigationProperty.GetValue(entity) is IEntityNavigationCollectionBase navigationCollection)
					{
						navigationCollection.SetConnection(connection);
					}
				}
			}
		}

#pragma warning disable CRR0026 // Unused member - called through Reflection
		private static void InitialiseSingleEntityRelationship<TRelatedEntity>(TEntity targetEntity, IEntityRelationship relationship, IMongoDbConnection connection) where TRelatedEntity : class
		{
			var dbEntityReader = new EntityReader<TRelatedEntity>(connection);
			var relationshipIdValue = relationship.IdProperty.GetValue(targetEntity);
			var loadedEntity = dbEntityReader.AsQueryable().WhereIdMatches(new[] { relationshipIdValue }).FirstOrDefault();
			relationship.NavigationProperty.SetValue(targetEntity, loadedEntity);
		}
#pragma warning restore CRR0026 // Unused member - called through Reflection
	}
}
