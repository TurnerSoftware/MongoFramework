using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure.Mutators
{
	public static class DbEntityMutator<TEntity>
	{
		public static List<IDbEntityMutator<TEntity>> Mutators { get; }

		static DbEntityMutator()
		{
			Mutators = new List<IDbEntityMutator<TEntity>>
			{
				new EntityAttributeMutator<TEntity>()
			};
		}
		
		public static void MutateEntity(TEntity entity, DbEntityMutatorType mutationType)
		{
			MutateEntities(new[] { entity }, mutationType);
		}

		public static void MutateEntities(IEnumerable<TEntity> entities, DbEntityMutatorType mutationType)
		{
			var entityMapper = new DbEntityMapper<TEntity>();

			foreach (var entity in entities)
			{
				foreach (var mutator in Mutators)
				{
					mutator.MutateEntity(entity, mutationType, entityMapper);
				}
			}
		}
	}
}
