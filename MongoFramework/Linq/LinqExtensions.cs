using MongoFramework.Infrastructure.Linq;
using MongoFramework.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MongoFramework.Linq
{
	public static class LinqExtensions
	{
		public static string ToQuery(this IQueryable queryable)
		{
			if (!(queryable is IMongoFrameworkQueryable))
			{
				throw new ArgumentException("Queryable must implement interface IMongoFrameworkQueryable", nameof(queryable));
			}

			return (queryable as IMongoFrameworkQueryable).ToQuery();
		}

		public static IQueryable<TEntity> WhereIdMatches<TEntity>(this IQueryable<TEntity> queryable, IEnumerable<object> entityIds)
		{
			var entityMapper = new EntityMapper<TEntity>();
			var idPropertyName = entityMapper.GetEntityMapping().Where(m => m.IsKey).Select(m => m.Property.Name).FirstOrDefault();
			
			//Dynamically build the LINQ query, it looks something like: e => entityIds.Contains(e.{idPropertyName})
			var entityParameter = Expression.Parameter(typeof(TEntity), "e");
			var idPropertyExpression = Expression.Property(entityParameter, idPropertyName);
			var entityIdsExpression = Expression.Constant(entityIds);
			var callExpression = Expression.Call(typeof(Enumerable), "Contains", new[] { typeof(object) }, entityIdsExpression, idPropertyExpression);
			var expression = Expression.Lambda<Func<TEntity, bool>>(
				Expression.Call(typeof(Enumerable), "Contains", new[] { typeof(object) }, entityIdsExpression, idPropertyExpression),
				entityParameter
			);
			
			return queryable.Where(expression);
		}
	}
}
