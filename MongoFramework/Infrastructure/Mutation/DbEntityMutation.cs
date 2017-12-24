using MongoFramework.Infrastructure.Mutation.Mutators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure.Mutation
{
	public static class DbEntityMutation<TEntity>
	{
		public static List<IDbEntityMutator<TEntity>> MutationDrivers { get; }

		static DbEntityMutation()
		{
			MutationDrivers = new List<IDbEntityMutator<TEntity>>
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
			var entityMapper = new DbEntityMapper<TEntity>();

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
