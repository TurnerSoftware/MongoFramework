using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MongoFramework.Infrastructure.Linq
{
	public interface IMongoFrameworkQueryProvider : IQueryProvider
	{
		IMongoDbConnection Connection { get; }
		Expression GetBaseExpression();
		string ToQuery(Expression expression);
	}

	public interface IMongoFrameworkQueryProvider<TEntity> : IMongoFrameworkQueryProvider where TEntity : class
	{
		EntityProcessorCollection<TEntity> EntityProcessors { get; }
	}
}
