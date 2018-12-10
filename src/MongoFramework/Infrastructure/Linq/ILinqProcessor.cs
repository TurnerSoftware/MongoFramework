namespace MongoFramework.Infrastructure.Linq
{
	public interface ILinqProcessor<TEntity> where TEntity : class
	{
		void ProcessEntity(TEntity entity);
	}
}
