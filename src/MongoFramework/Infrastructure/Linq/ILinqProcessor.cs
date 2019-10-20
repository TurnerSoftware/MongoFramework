namespace MongoFramework.Infrastructure.Linq
{
	public interface ILinqProcessor<in TEntity> where TEntity : class
	{
		void ProcessEntity(TEntity entity, IMongoDbConnection connection);
	}
}
