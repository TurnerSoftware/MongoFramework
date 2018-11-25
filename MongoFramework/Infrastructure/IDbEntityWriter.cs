using System.Threading;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure
{
	public interface IDbEntityWriter<TEntity> where TEntity : class
	{
		void Write(IDbEntityCollection<TEntity> entityCollection);
		Task WriteAsync(IDbEntityCollection<TEntity> entityCollection, CancellationToken cancellationToken = default(CancellationToken));
	}
}
