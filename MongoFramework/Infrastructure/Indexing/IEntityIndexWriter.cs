using System.Threading.Tasks;

namespace MongoFramework.Infrastructure.Indexing
{
	public interface IEntityIndexWriter<TEntity>
	{
		void ApplyIndexing();
		Task ApplyIndexingAsync();
	}
}
