using MongoDB.Driver;
using MongoFramework.Infrastructure.EntityRelationships;
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
			var relationships = connection.GetEntityMapper(typeof(TEntity)).GetEntityRelationships(connection);
			foreach (var relationship in relationships)
			{
				if (mutationType == MutatorType.Select)
				{
					var initialiseSingleEntityMethod = GetType().GetMethod("InitialiseSingleEntityRelationship", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(relationship.EntityType);
					initialiseSingleEntityMethod.Invoke(entity, new object[] { entity, relationship, connection });
				}
				else if (mutationType == MutatorType.Create && relationship.IsCollection)
				{
					var navigationCollectionType = typeof(EntityNavigationCollection<>).MakeGenericType(relationship.EntityType);
					var navigationCollection = Activator.CreateInstance(navigationCollectionType, relationship.IdProperty.Name, connection) as IEntityNavigationCollection;
					relationship.NavigationProperty.SetValue(entity, navigationCollection);
				}
			}
		}

#pragma warning disable CRR0026 // Unused member - called through Reflection
		private static void InitialiseSingleEntityRelationship<TRelatedEntity>(TEntity targetEntity, EntityRelationship relationship, IMongoDbConnection connection) where TRelatedEntity : class
		{
			var dbEntityReader = new EntityReader<TRelatedEntity>(connection);
			var relationshipIdValue = relationship.IdProperty.GetValue(targetEntity);
			var loadedEntity = dbEntityReader.AsQueryable().WhereIdMatches(new[] { relationshipIdValue }).FirstOrDefault();
			relationship.NavigationProperty.SetValue(targetEntity, loadedEntity);
		}
#pragma warning restore CRR0026 // Unused member - called through Reflection
	}
}
