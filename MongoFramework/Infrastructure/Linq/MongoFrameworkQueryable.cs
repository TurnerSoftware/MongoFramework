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
		private IMongoFrameworkQueryProvider<TEntity, TOutput> internalProvider { get; set; }
		
		public Type ElementType => typeof(TOutput);
		public Expression Expression { get; }
		public IQueryProvider Provider => internalProvider;

		public EntityProcessorCollection<TEntity> EntityProcessors => internalProvider.EntityProcessors;

		public MongoFrameworkQueryable(IMongoQueryable<TOutput> underlyingQueryable)
		{
			internalProvider = new MongoFrameworkQueryProvider<TEntity, TOutput>(underlyingQueryable);
			Expression = Expression.Constant(underlyingQueryable, typeof(IMongoQueryable<TOutput>));
		}

		public MongoFrameworkQueryable(IMongoQueryable<TOutput> underlyingQueryable, Expression expression)
		{
			internalProvider = new MongoFrameworkQueryProvider<TEntity, TOutput>(underlyingQueryable);
			Expression = expression;
		}

		public MongoFrameworkQueryable(IMongoQueryable<TOutput> underlyingQueryable, Expression expression, EntityProcessorCollection<TEntity> queryableProcessor)
		{
			internalProvider = new MongoFrameworkQueryProvider<TEntity, TOutput>(underlyingQueryable);
			internalProvider.EntityProcessors.AddRange(queryableProcessor);
			Expression = expression;
		}

		public IEnumerator<TOutput> GetEnumerator()
		{
			var result = (IEnumerable<TOutput>)internalProvider.Execute(Expression);
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
