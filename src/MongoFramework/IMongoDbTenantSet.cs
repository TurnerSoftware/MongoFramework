namespace MongoFramework
{
	public interface IMongoDbTenantSet<TEntity> : IMongoDbSet<TEntity> where TEntity : class
	{
		new IMongoDbTenantContext Context { get; }
	}
}
