using MongoFramework.Infrastructure.Mutation;

namespace MongoFramework.Infrastructure.Linq.Processors
{
	public class EntityMutationProcessor<TEntity> : ILinqProcessor<TEntity> where TEntity : class
	{
		public IMongoDbConnection Connection { get; private set; }

		public EntityMutationProcessor(IMongoDbConnection connection)
		{
			Connection = connection;
		}

		public void ProcessEntity(TEntity entity)
		{
			EntityMutation<TEntity>.MutateEntity(entity, MutatorType.Select, Connection);
		}
	}
}
