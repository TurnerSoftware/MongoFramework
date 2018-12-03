using MongoFramework.Infrastructure.Linq;
using MongoFramework.Infrastructure.Mapping;
using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MongoFramework.Linq
{
	public static class LinqExtensions
	{
		public static string ToQuery(this IQueryable queryable)
		{
			if (!(queryable is IMongoFrameworkQueryable))
			{
				throw new ArgumentException($"Queryable must implement interface {nameof(IMongoFrameworkQueryable)}", nameof(queryable));
			}

			return (queryable as IMongoFrameworkQueryable).ToQuery();
		}

		public static IQueryable<TEntity> WhereIdMatches<TEntity>(this IQueryable<TEntity> queryable, IEnumerable entityIds) where TEntity : class
		{
			if (queryable is IMongoFrameworkQueryable mongoDbQueryable)
			{
				var entityMapper = mongoDbQueryable.Connection.GetEntityMapper(typeof(TEntity));
				var idProperty = entityMapper.GetEntityMapping().Where(m => m.IsKey).Select(m => m.Property).FirstOrDefault();
				return queryable.WherePropertyMatches(idProperty.Name, idProperty.PropertyType, entityIds);
			}

			//TODO: Look at potentially avoiding the EntityMapper by looking at the BsonClassMapSerializer instead?
			throw new ArgumentException($"Queryable must implement interface {nameof(IMongoFrameworkQueryable)}", nameof(queryable));
		}

		public static IQueryable<TEntity> WherePropertyMatches<TEntity>(this IQueryable<TEntity> queryable, string propertyName, Type propertyType, IEnumerable identifiers) where TEntity : class
		{
			//The cast allows for handling identifiers generically as "IEnumerable<object>". Without the Cast call, we can't handle ObjectId identifiers.
			var castMethod = typeof(Enumerable).GetMethod("Cast", BindingFlags.Public | BindingFlags.Static);
			var castedIdentifiers = castMethod.MakeGenericMethod(propertyType).Invoke(null, new[] { identifiers });

			//Dynamically build the LINQ query, it looks something like: e => castedIdentifiers.Contains(e.{propertyName})
			var entityParameter = Expression.Parameter(typeof(TEntity), "e");
			var propertyExpression = Expression.Property(entityParameter, propertyName);
			var identifiersExpression = Expression.Constant(castedIdentifiers);
			var expression = Expression.Lambda<Func<TEntity, bool>>(
				Expression.Call(typeof(Enumerable), "Contains", new[] { propertyType }, identifiersExpression, propertyExpression),
				entityParameter
			);

			return queryable.Where(expression);
		}
	}
}
