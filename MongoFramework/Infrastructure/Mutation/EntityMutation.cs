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

		public static void MutateEntity(TEntity entity, MutatorType mutationType, IDbContextSettings settings)
		{
			MutateEntities(new[] { entity }, mutationType, settings);
		}

		public static void MutateEntities(IEnumerable<TEntity> entities, MutatorType mutationType, IDbContextSettings settings)
		{
			foreach (var entity in entities)
			{
				foreach (var driver in MutationDrivers)
				{
					driver.MutateEntity(entity, mutationType, settings);
				}
			}
		}
	}
}
