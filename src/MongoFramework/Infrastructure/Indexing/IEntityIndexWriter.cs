using System.Threading;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure.Indexing
{
	public interface IEntityIndexWriter<TEntity> where TEntity : class
	{
		void ApplyIndexing();
		Task ApplyIndexingAsync(CancellationToken cancellationToken = default(CancellationToken));
	}
}
