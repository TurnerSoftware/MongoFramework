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
			var idProperty = EntityMapping.GetOrCreateDefinition(typeof(TEntity))
				.GetAllProperties()
				.Where(p => p.IsKey)
				.FirstOrDefault();

			return queryable.WherePropertyMatches(idProperty, entityIds);
		}

		public static IQueryable<TEntity> WherePropertyMatches<TEntity>(this IQueryable<TEntity> queryable, IEntityProperty property, IEnumerable values) where TEntity : class
		{
			//The cast allows for handling identifiers generically as "IEnumerable<object>". Without the Cast call, we can't handle ObjectId etc.
			var castMethod = typeof(Enumerable).GetMethod("Cast", BindingFlags.Public | BindingFlags.Static);
			var castedIdentifiers = castMethod.MakeGenericMethod(property.PropertyType).Invoke(null, new[] { values });

			//Dynamically build the LINQ query, it looks something like: e => castedIdentifiers.Contains(e.{propertyName})
			var entityParameter = Expression.Parameter(typeof(TEntity), "e");
			var propertyExpression = Expression.Property(entityParameter, property.PropertyInfo.Name);
			var identifiersExpression = Expression.Constant(castedIdentifiers);
			var expression = Expression.Lambda<Func<TEntity, bool>>(
				Expression.Call(typeof(Enumerable), "Contains", new[] { property.PropertyType }, identifiersExpression, propertyExpression),
				entityParameter
			);

			return queryable.Where(expression);
		}
	}
}
