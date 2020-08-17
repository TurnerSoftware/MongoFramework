using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using MongoFramework.Attributes;

namespace MongoFramework.Infrastructure.Internal
{
	public static class DbSetInitializer
	{
		private static readonly ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> PropertyCache = new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();
		private static readonly ConcurrentDictionary<Type, (bool WithOptions, Delegate ConstructorDelegate)> ConstructorCache = new ConcurrentDictionary<Type, (bool, Delegate)>();

		public static IEnumerable<PropertyInfo> GetDbSetProperties(IMongoDbContext context)
		{
			var contextType = context.GetType();
			return PropertyCache.GetOrAdd(contextType, targetType =>
			{
				var allProperties = targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
				var dbSetProperties = new List<PropertyInfo>();
				var mongoDbSetType = typeof(IMongoDbSet);

				foreach (var property in allProperties)
				{
					var propertyType = property.PropertyType;
					if (propertyType.IsGenericType && mongoDbSetType.IsAssignableFrom(propertyType))
					{
						dbSetProperties.Add(property);
					}
				}

				return dbSetProperties;
			});
		}

		public static IDbSetOptions GetDefaultDbSetOptions(PropertyInfo propertyInfo)
		{
			var optionsAttribute = propertyInfo.GetCustomAttribute<DbSetOptionsAttribute>();
			return optionsAttribute?.GetOptions();
		}

		public static IMongoDbSet CreateDbSet(Type dbSetType, IMongoDbContext context, IDbSetOptions options = null)
		{
			var constructorDetails = ConstructorCache.GetOrAdd(dbSetType, type =>
			{
				var constructorWithOptions = dbSetType.GetConstructor(new[] { typeof(IMongoDbContext), typeof(IDbSetOptions) });
				if (constructorWithOptions != null)
				{
					var parameters = new[] { Expression.Parameter(typeof(IMongoDbContext)), Expression.Parameter(typeof(IDbSetOptions)) };
					var constructorDelegate = Expression.Lambda<Func<IMongoDbContext, IDbSetOptions, IMongoDbSet>>(
						Expression.Convert(
							Expression.New(constructorWithOptions, parameters),
							typeof(IMongoDbSet)
						),
						parameters
					).Compile();
					return (true, constructorDelegate);
				}
				else
				{
					var constructorNoOptions = dbSetType.GetConstructor(new[] { typeof(IMongoDbContext) });
					if (constructorNoOptions == null)
					{
						throw new InvalidOperationException("No valid constructor available for IMongoDbSet");
					}

					var parameters = new[] { Expression.Parameter(typeof(IMongoDbContext)) };
					var constructorDelegate = Expression.Lambda<Func<IMongoDbContext, IMongoDbSet>>(
						Expression.Convert(
							Expression.New(constructorNoOptions, parameters),
							typeof(IMongoDbSet)
						),
						parameters
					).Compile();
					return (false, constructorDelegate);
				}
			});

			if (constructorDetails.WithOptions)
			{
				var constructor = constructorDetails.ConstructorDelegate as Func<IMongoDbContext, IDbSetOptions, IMongoDbSet>;
				return constructor(context, options);
			}
			else
			{
				var constructor = constructorDetails.ConstructorDelegate as Func<IMongoDbContext, IMongoDbSet>;
				return constructor(context);
			}
		}
	}
}
