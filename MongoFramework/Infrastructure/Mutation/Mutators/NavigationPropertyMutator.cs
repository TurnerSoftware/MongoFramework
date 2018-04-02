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
			if (mutationType == MutatorType.Select)
			{
				foreach (var relationship in relationships)
				{
					EntityRelationshipHelper.LoadNavigationProperty(entity, relationship, database);
				}
			}
			else
			{
				foreach (var relationship in relationships)
				{
					EntityRelationshipHelper.SaveNavigationProperty(entity, relationship, database);
				}
			}
		}
	}
}
