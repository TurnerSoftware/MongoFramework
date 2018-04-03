﻿using MongoDB.Driver;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mutation.Mutators;
using System.Collections.Generic;

namespace MongoFramework.Infrastructure.Mutation
{
	public static class EntityMutation<TEntity>
	{
		public static List<IEntityMutator<TEntity>> MutationDrivers { get; }

		static EntityMutation()
		{
			MutationDrivers = new List<IEntityMutator<TEntity>>
			{
				new EntityAttributeMutator<TEntity>(),
				new NavigationPropertyMutator<TEntity>()
			};
		}

		public static void MutateEntity(TEntity entity, MutatorType mutationType, IMongoDatabase database)
		{
			MutateEntities(new[] { entity }, mutationType, database);
		}

		public static void MutateEntities(IEnumerable<TEntity> entities, MutatorType mutationType, IMongoDatabase database)
		{
			var entityMapper = new EntityMapper<TEntity>();

			foreach (var entity in entities)
			{
				foreach (var driver in MutationDrivers)
				{
					driver.MutateEntity(entity, mutationType, entityMapper, database);
				}
			}
		}
	}
}
