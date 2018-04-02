using System.Threading.Tasks;

namespace MongoFramework.Infrastructure
{
	public interface IDbEntityWriter<TEntity>
	{
		void Write(IDbEntityCollection<TEntity> entityCollection);
		Task WriteAsync(IDbEntityCollection<TEntity> entityCollection);
	}
}
