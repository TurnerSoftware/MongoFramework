using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Mapping;
using System.Reflection;
using System.ComponentModel.DataAnnotations.Schema;
using MongoFramework.Infrastructure.EntityRelationships;

namespace MongoFramework.Infrastructure.Mutation.Mutators
{
	public class NavigationPropertyMutator<TEntity> : IEntityMutator<TEntity>
	{
		public void MutateEntity(TEntity entity, MutatorType mutationType, IEntityMapper entityMapper, IMongoDatabase database)
		{
			if (database == null)
			{
				throw new ArgumentNullException(nameof(database));
			}

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
