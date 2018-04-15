using System.Threading;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure
{
	public interface IDbEntityWriter<TEntity>
	{
		void Write(IDbEntityCollection<TEntity> entityCollection);
		Task WriteAsync(IDbEntityCollection<TEntity> entityCollection, CancellationToken cancellationToken = default(CancellationToken));
	}
}
