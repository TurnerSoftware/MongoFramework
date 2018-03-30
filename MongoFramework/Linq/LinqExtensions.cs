using MongoFramework.Infrastructure.Linq;
using System;
using System.Linq;

namespace MongoFramework.Linq
{
	public static class LinqExtensions
	{
		public static string ToQuery(this IQueryable queryable)
		{
			if (!(queryable is IMongoFrameworkQueryable))
			{
				throw new ArgumentException("Queryable must implement interface IMongoFrameworkQueryable", "queryable");
			}

			return (queryable as IMongoFrameworkQueryable).ToQuery();
		}
	}
}
