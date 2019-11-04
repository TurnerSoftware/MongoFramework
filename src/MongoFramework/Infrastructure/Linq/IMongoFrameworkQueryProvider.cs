using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure.Linq
{
	public interface IMongoFrameworkQueryProvider : IQueryProvider
	{
		IMongoDbConnection Connection { get; }
		Expression GetBaseExpression();
		object ExecuteAsync(Expression expression, CancellationToken cancellationToken = default);
		string ToQuery(Expression expression);
	}

	public interface IMongoFrameworkQueryProvider<TEntity> : IMongoFrameworkQueryProvider where TEntity : class
	{
		EntityProcessorCollection<TEntity> EntityProcessors { get; }
		IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default);
	}
}
