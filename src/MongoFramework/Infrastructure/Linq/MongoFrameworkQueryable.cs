using MongoDB.Driver.Linq;
using MongoFramework.Infrastructure.Diagnostics;
using MongoFramework.Infrastructure.Mapping;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace MongoFramework.Infrastructure.Linq
{
	public class MongoFrameworkQueryable<TOutput> : IMongoFrameworkQueryable<TOutput>
	{
		private IMongoFrameworkQueryProvider InternalProvider { get; }

		public Type ElementType => typeof(TOutput);
		public Expression Expression { get; }
		public IQueryProvider Provider => InternalProvider;

		public MongoFrameworkQueryable(IMongoFrameworkQueryProvider provider)
		{
			InternalProvider = provider;
			Expression = provider.GetBaseExpression();
		}

		public MongoFrameworkQueryable(IMongoFrameworkQueryProvider provider, Expression expression)
		{
			InternalProvider = provider;
			Expression = expression;
		}

		public IEnumerator<TOutput> GetEnumerator()
		{
			var result = (IEnumerable<TOutput>)InternalProvider.Execute(Expression);
			return result.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public string ToQuery()
		{
			return InternalProvider.ToQuery(Expression);
		}

		public IAsyncEnumerable<TOutput> AsAsyncEnumerable(CancellationToken cancellationToken = default)
		{
			return (IAsyncEnumerable<TOutput>)InternalProvider.ExecuteAsync(Expression, cancellationToken);
		}
	}
}
