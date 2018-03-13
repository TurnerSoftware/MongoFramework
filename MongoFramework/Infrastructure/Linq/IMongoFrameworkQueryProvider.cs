using System.Linq;
using MongoDB.Driver.Linq;
using MongoFramework.Infrastructure.Linq.Processors;

namespace MongoFramework.Infrastructure.Linq
{
	public interface IMongoFrameworkQueryProvider<TEntity, TOutput> : IQueryProvider
	{
		IMongoQueryable UnderlyingQueryable { get; }
		EntityProcessorCollection<TEntity> EntityProcessors { get; }
	}
}
