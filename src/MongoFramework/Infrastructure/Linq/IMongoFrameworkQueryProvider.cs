using MongoDB.Driver.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MongoFramework.Infrastructure.Linq
{
	public interface IMongoFrameworkQueryProvider<TEntity, TOutput> : IQueryProvider where TEntity : class
	{
		IMongoQueryable UnderlyingQueryable { get; }
		EntityProcessorCollection<TEntity> EntityProcessors { get; }
		IEnumerable<TOutput> ExecuteEnumerable(Expression expression);
		string ToQuery();
	}
}
