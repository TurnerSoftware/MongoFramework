using System.Threading.Tasks;

namespace MongoFramework.Infrastructure
{
	public interface IDbEntityWriter<TEntity>
	{
		void Write(IDbEntityContainer<TEntity> entityContainer);
		Task WriteAsync(IDbEntityContainer<TEntity> entityContainer);
	}
}
