using MongoDB.Driver.Linq;
using MongoFramework.Infrastructure.Linq.Processors;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace MongoFramework.Infrastructure.Linq
{
	public class MongoFrameworkQueryProvider<TEntity, TOutput> : IMongoFrameworkQueryProvider<TEntity, TOutput> where TEntity : class
	{
		private IMongoDbConnection Connection { get; }
		public IMongoQueryable UnderlyingQueryable { get; }

		public EntityProcessorCollection<TEntity> EntityProcessors { get; } = new EntityProcessorCollection<TEntity>();

		public MongoFrameworkQueryProvider(IMongoDbConnection connection, IMongoQueryable<TOutput> underlyingQueryable)
		{
			Connection = connection;
			UnderlyingQueryable = underlyingQueryable;
		}

		public IQueryable CreateQuery(Expression expression)
		{
			throw new NotImplementedException();
		}

		public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
		{
			var newUnderlyingQueryable = (IMongoQueryable<TElement>)UnderlyingQueryable.Provider.CreateQuery<TElement>(expression);
			var queryable = new MongoFrameworkQueryable<TEntity, TElement>(Connection, newUnderlyingQueryable, expression);
			queryable.EntityProcessors.AddRange(EntityProcessors);
			return queryable;
		}

		public virtual object Execute(Expression expression)
		{
			var result = UnderlyingQueryable.Provider.Execute(expression);

			if (result is TEntity)
			{
				EntityProcessors.ProcessEntity((TEntity)result);
			}

			return result;
		}

		public virtual TResult Execute<TResult>(Expression expression)
		{
			var result = Execute(expression);
			return (TResult)result;
		}
	}
}
