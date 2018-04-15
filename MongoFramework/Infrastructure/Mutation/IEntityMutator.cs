using MongoDB.Driver;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Infrastructure.Mutation
{
	public interface IEntityMutator<TEntity>
	{
		void MutateEntity(TEntity entity, MutatorType mutationType, IEntityMapper entityMapper, IMongoDatabase database);
	}
}
