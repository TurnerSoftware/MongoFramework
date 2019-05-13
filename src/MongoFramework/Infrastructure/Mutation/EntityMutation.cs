using MongoFramework.Infrastructure.Mutation.Mutators;
using System.Collections.Generic;

namespace MongoFramework.Infrastructure.Mutation
{
	public static class EntityMutation<TEntity> where TEntity : class
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

		public static void MutateEntity(TEntity entity, MutatorType mutationType, IMongoDbConnection connection)
		{
			MutateEntities(new[] { entity }, mutationType, connection);
		}

		public static void MutateEntities(IEnumerable<TEntity> entities, MutatorType mutationType, IMongoDbConnection connection)
		{
			foreach (var entity in entities)
			{
				foreach (var driver in MutationDrivers)
				{
					driver.MutateEntity(entity, mutationType, connection);
				}
			}
		}

		public static void MutateEntities(IEntityCollection<TEntity> entities, IMongoDbConnection connection)
		{
			foreach (var entry in entities.GetEntries())
			{
				if (entry.State == EntityEntryState.Added)
				{
					MutateEntity(entry.Entity, MutatorType.Insert, connection);
				}
				else if (entry.State == EntityEntryState.Updated)
				{
					MutateEntity(entry.Entity, MutatorType.Update, connection);
				}
			}
		}
	}
}
