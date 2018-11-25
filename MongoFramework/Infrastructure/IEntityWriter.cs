using System.Threading;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure
{
	public interface IEntityWriter<TEntity> where TEntity : class
	{
		void Write(IEntityCollection<TEntity> entityCollection);
		Task WriteAsync(IEntityCollection<TEntity> entityCollection, CancellationToken cancellationToken = default(CancellationToken));
	}
}
