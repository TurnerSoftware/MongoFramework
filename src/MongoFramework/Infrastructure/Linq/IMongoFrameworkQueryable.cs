using System.Linq;

namespace MongoFramework.Infrastructure.Linq
{
	public interface IMongoFrameworkQueryable : IOrderedQueryable
	{
		string ToQuery();
	}

	public interface IMongoFrameworkQueryable<TOutput> : IMongoFrameworkQueryable, IOrderedQueryable<TOutput>
	{
	}
}
