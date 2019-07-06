using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using MongoDB.Driver.Linq;
using MongoFramework.Infrastructure.Linq;
using MongoFramework.Infrastructure.Mapping;
using System;
using System.Collections;
using System.Collections.Generic;
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

		private static IQueryable<TEntity> WhereFilter<TEntity>(this IQueryable<TEntity> queryable, Func<FilterDefinitionBuilder<TEntity>, FilterDefinition<TEntity>> queryFilter)
		{
			var definition = queryFilter.Invoke(Builders<TEntity>.Filter);
			return queryable.Where(e => definition.Inject());
		}

		public static IQueryable<TEntity> SearchText<TEntity>(this IMongoDbSet<TEntity> dbSet, string search) where TEntity : class
		{
			return dbSet.WhereFilter(b => b.Text(search));
		}

		public static IQueryable<TEntity> SearchGeoIntersecting<TEntity, TCoordinates>(this IQueryable<TEntity> queryable, Expression<Func<TEntity, object>> field, GeoJsonGeometry<TCoordinates> geometry) where TCoordinates : GeoJsonCoordinates
		{
			return queryable.WhereFilter(b => b.GeoIntersects(field, geometry));
		}


		public static IQueryable<TEntity> SearchGeoNear<TEntity, TCoordinates>(this IMongoDbSet<TEntity> dbSet, Expression<Func<TEntity, object>> targetField, GeoJsonPoint<TCoordinates> point, Expression<Func<TEntity, object>> distanceResultField = null, double? maxDistance = null, double? minDistance = null) where TEntity : class where TCoordinates : GeoJsonCoordinates
		{
			var entitySerializer = BsonSerializer.LookupSerializer<TEntity>();
			var keyExpressionField = new ExpressionFieldDefinition<TEntity>(targetField);
			var keyStringField = keyExpressionField.Render(entitySerializer, BsonSerializer.SerializerRegistry);

			var distanceFieldName = "Distance";
			if (distanceResultField != null)
			{
				var distanceResultExpressionField = new ExpressionFieldDefinition<TEntity>(distanceResultField);
				var distanceResultStringField = distanceResultExpressionField.Render(entitySerializer, BsonSerializer.SerializerRegistry);
				distanceFieldName = distanceResultStringField.FieldName;
			}
			
			var geoNearSettings = new BsonDocument
			{
				{ "near", point.ToBsonDocument() },
				{ "key", keyStringField.FieldName },
				{ "distanceField", distanceFieldName }
			};

			if (maxDistance.HasValue)
			{
				geoNearSettings.Add("maxDistance", maxDistance.Value);
			}
			if (minDistance.HasValue)
			{
				geoNearSettings.Add("minDistance", minDistance.Value);
			}

			var stage = new BsonDocument
			{
				{ "$geoNear", geoNearSettings }
			};

			var originalProvider = dbSet.Provider as IMongoFrameworkQueryProvider<TEntity>;
			var provider = new MongoFrameworkQueryProvider<TEntity>(originalProvider, stage);
			return new MongoFrameworkQueryable<TEntity>(provider);
		}
	}
}
