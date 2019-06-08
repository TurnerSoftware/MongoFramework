using MongoDB.Driver.Linq;
using MongoFramework.Infrastructure.Diagnostics;
using MongoFramework.Infrastructure.Mapping;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MongoFramework.Infrastructure.Linq
{
	public class MongoFrameworkQueryable<TEntity, TOutput> : IMongoFrameworkQueryable<TEntity, TOutput> where TEntity : class
	{
		private IMongoFrameworkQueryProvider<TEntity, TOutput> InternalProvider { get; set; }

		public IMongoDbConnection Connection { get; }
		public Type ElementType => typeof(TOutput);
		public Expression Expression { get; }
		public IQueryProvider Provider => InternalProvider;

		public EntityProcessorCollection<TEntity> EntityProcessors => InternalProvider.EntityProcessors;

		public MongoFrameworkQueryable(IMongoDbConnection connection, IMongoQueryable<TOutput> underlyingQueryable)
		{
			Connection = connection;
			InternalProvider = new MongoFrameworkQueryProvider<TEntity, TOutput>(connection, underlyingQueryable);
			Expression = Expression.Constant(underlyingQueryable, typeof(IMongoQueryable<TOutput>));
		}

		public MongoFrameworkQueryable(IMongoDbConnection connection, IMongoQueryable<TOutput> underlyingQueryable, Expression expression)
		{
			Connection = connection;
			InternalProvider = new MongoFrameworkQueryProvider<TEntity, TOutput>(connection, underlyingQueryable);
			Expression = expression;
		}

		public IEnumerator<TOutput> GetEnumerator()
		{
			return InternalProvider.ExecuteEnumerable(Expression).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public string ToQuery()
		{
			return InternalProvider.ToQuery();
		}
	}
}
