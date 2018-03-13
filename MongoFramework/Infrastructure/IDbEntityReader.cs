using System.Linq;

namespace MongoFramework.Infrastructure
{
	public interface IDbEntityReader<TEntity>
	{
		IQueryable<TEntity> AsQueryable();
	}
}
