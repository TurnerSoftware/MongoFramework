using System.Threading.Tasks;

namespace MongoFramework
{
	public interface IMongoDbContext
	{
		void SaveChanges();
		Task SaveChangesAsync();
	}
}
