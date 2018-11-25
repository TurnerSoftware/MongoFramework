using System.Linq;

namespace MongoFramework.Infrastructure
{
	public interface IDbEntityReader<TEntity> where TEntity : class
	{
		IQueryable<TEntity> AsQueryable();
	}
}
