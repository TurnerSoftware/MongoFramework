using System.Linq;

namespace MongoFramework.Infrastructure
{
	public interface IEntityReader<TEntity> where TEntity : class
	{
		IQueryable<TEntity> AsQueryable();
	}
}
