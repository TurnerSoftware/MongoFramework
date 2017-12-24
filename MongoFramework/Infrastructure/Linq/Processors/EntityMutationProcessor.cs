using MongoFramework.Infrastructure.Mutation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure.Linq.Processors
{
	public class EntityMutationProcessor<TEntity> : IEntityProcessor<TEntity>
	{
		public void ProcessEntity(TEntity entity)
		{
			DbEntityMutation<TEntity>.MutateEntity(entity, MutatorType.Select);
		}
	}
}
