using MongoDB.Driver.Linq;
using MongoFramework.Infrastructure.Linq.Processors;
using System.Linq;

namespace MongoFramework.Infrastructure.Linq
{
	public interface IMongoFrameworkQueryProvider<TEntity, TOutput> : IQueryProvider
	{
		IMongoQueryable UnderlyingQueryable { get; }
		EntityProcessorCollection<TEntity> EntityProcessors { get; }
	}
}
