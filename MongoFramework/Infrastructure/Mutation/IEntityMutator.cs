using MongoDB.Driver;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Infrastructure.Mutation
{
	public interface IEntityMutator<TEntity> where TEntity : class
	{
		void MutateEntity(TEntity entity, MutatorType mutationType, IMongoDbConnection connection);
	}
}
