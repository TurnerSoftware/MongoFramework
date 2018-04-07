using MongoDB.Driver;
using MongoFramework.Infrastructure.EntityRelationships;
using MongoFramework.Infrastructure.Mapping;
using System;
using System.Linq;

namespace MongoFramework.Infrastructure.Mutation.Mutators
{
	public class NavigationPropertyMutator<TEntity> : IEntityMutator<TEntity>
	{
		public void MutateEntity(TEntity entity, MutatorType mutationType, IEntityMapper entityMapper, IMongoDatabase database)
		{
			var relationships = EntityRelationshipHelper.GetRelationshipsForType(typeof(TEntity));

			foreach (var relationship in relationships)
			{
				if (mutationType == MutatorType.Select)
				{
					EntityRelationshipHelper.LoadNavigationProperty(entity, relationship, database);
				}
				else if (mutationType == MutatorType.Create)
				{
					EntityRelationshipHelper.InitialiseNavigationProperty(entity, relationship);
				}
				else
				{
					EntityRelationshipHelper.SaveNavigationProperty(entity, relationship, database);
				}
			}
		}
	}
}
