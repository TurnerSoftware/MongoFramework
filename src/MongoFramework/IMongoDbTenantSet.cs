using System.Linq;

namespace MongoFramework
{
	public interface IMongoDbTenantSet<TEntity> : IMongoDbSet<TEntity> where TEntity : class
	{
		new IMongoDbTenantContext Context { get; }
		IQueryable<TEntity> GetSearchTextQueryable(string search);
	}
}
