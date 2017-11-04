using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using System.Collections;
using System.Linq.Expressions;
using System.Threading;
using MongoFramework.Infrastructure.Linq.Processors;

namespace MongoFramework.Infrastructure.Linq
{
	public class MongoFrameworkQueryable<TEntity, TOutput> : IMongoFrameworkQueryable<TEntity, TOutput>
	{
		private IMongoFrameworkQueryProvider<TEntity, TOutput> InternalProvider { get; set; }
		
		public Type ElementType => typeof(TOutput);
		public Expression Expression { get; }
		public IQueryProvider Provider => InternalProvider;

		public EntityProcessorCollection<TEntity> EntityProcessors => InternalProvider.EntityProcessors;

		public MongoFrameworkQueryable(IMongoQueryable<TOutput> underlyingQueryable)
		{
			InternalProvider = new MongoFrameworkQueryProvider<TEntity, TOutput>(underlyingQueryable);
			Expression = Expression.Constant(underlyingQueryable, typeof(IMongoQueryable<TOutput>));
		}

		public MongoFrameworkQueryable(IMongoQueryable<TOutput> underlyingQueryable, Expression expression)
		{
			InternalProvider = new MongoFrameworkQueryProvider<TEntity, TOutput>(underlyingQueryable);
			Expression = expression;
		}

		public MongoFrameworkQueryable(IMongoQueryable<TOutput> underlyingQueryable, Expression expression, EntityProcessorCollection<TEntity> queryableProcessor)
		{
			InternalProvider = new MongoFrameworkQueryProvider<TEntity, TOutput>(underlyingQueryable);
			InternalProvider.EntityProcessors.AddRange(queryableProcessor);
			Expression = expression;
		}

		public IEnumerator<TOutput> GetEnumerator()
		{
			var result = (IEnumerable<TOutput>)InternalProvider.Execute(Expression);
			using (var enumerator = result.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					var item = enumerator.Current;
					if (item is TEntity)
					{
						EntityProcessors.ProcessEntity((TEntity)(object)item);
					}
					yield return item;
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
