using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mutation.Mutators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure.Mutation
{
	public static class EntityMutation<TEntity>
	{
		public static List<IEntityMutator<TEntity>> MutationDrivers { get; }

		static EntityMutation()
		{
			MutationDrivers = new List<IEntityMutator<TEntity>>
			{
				new EntityAttributeMutator<TEntity>()
			};
		}
		
		public static void MutateEntity(TEntity entity, MutatorType mutationType)
		{
			MutateEntities(new[] { entity }, mutationType);
		}

		public static void MutateEntities(IEnumerable<TEntity> entities, MutatorType mutationType)
		{
			var entityMapper = new EntityMapper<TEntity>();

			foreach (var entity in entities)
			{
				foreach (var driver in MutationDrivers)
				{
					driver.MutateEntity(entity, mutationType, entityMapper);
				}
			}
		}
	}
}
