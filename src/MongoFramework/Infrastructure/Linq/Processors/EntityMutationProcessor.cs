using MongoFramework.Infrastructure.Mutation;

namespace MongoFramework.Infrastructure.Linq.Processors
{
	public class EntityMutationProcessor<TEntity> : ILinqProcessor<TEntity> where TEntity : class
	{
		public void ProcessEntity(TEntity entity, IMongoDbConnection connection)
		{
			EntityMutation<TEntity>.MutateEntity(entity, MutatorType.Select, connection);
		}
	}
}
