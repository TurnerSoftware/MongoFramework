namespace MongoFramework.Infrastructure.Linq
{
	public interface ILinqProcessor<TEntity>
	{
		void ProcessEntity(TEntity entity);
	}
}
