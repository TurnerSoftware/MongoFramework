using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace MongoFramework
{
	public interface IMongoDbContext
	{
		void SaveChanges();
		Task SaveChangesAsync(CancellationToken cancellationToken = default);
	}
}
