using MongoFramework.Infrastructure.Mutation;

namespace MongoFramework.Infrastructure.Linq.Processors
{
	public class EntityMutationProcessor<TEntity> : ILinqProcessor<TEntity>
	{
		public void ProcessEntity(TEntity entity)
		{
			EntityMutation<TEntity>.MutateEntity(entity, MutatorType.Select);
		}
	}
}
