using System.Linq;
using MongoFramework.Infrastructure.Linq.Processors;

namespace MongoFramework.Infrastructure.Linq
{
	public interface IMongoFrameworkQueryable : IOrderedQueryable
	{
		string ToQuery();
	}

	public interface IMongoFrameworkQueryable<TEntity, TOutput> : IMongoFrameworkQueryable, IOrderedQueryable<TOutput>
	{
		EntityProcessorCollection<TEntity> EntityProcessors { get; }
	}
}
