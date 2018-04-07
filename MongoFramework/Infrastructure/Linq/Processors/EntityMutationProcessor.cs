using MongoDB.Driver;
using MongoFramework.Infrastructure.Mutation;

namespace MongoFramework.Infrastructure.Linq.Processors
{
	public class EntityMutationProcessor<TEntity> : ILinqProcessor<TEntity>
	{
		public IMongoDatabase Database { get; private set; }

		public EntityMutationProcessor(IMongoDatabase database)
		{
			Database = database;
		}

		public void ProcessEntity(TEntity entity)
		{
			EntityMutation<TEntity>.MutateEntity(entity, MutatorType.Select, Database);
		}
	}
}
