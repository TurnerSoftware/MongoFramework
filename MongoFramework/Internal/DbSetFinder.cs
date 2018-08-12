using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MongoFramework.Internal
{
	public class DbSetFinder : IDbSetFinder
	{
		public IEnumerable<DbSetInfo> FindSets(Type contextType)
		{
			if (!contextType.IsAssignableFrom(typeof(IMongoDbContext)))
			{
				throw new ArgumentException("Context type must be assignable from IMongoDbContext");
			}

			var properties = contextType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			var mongoDbSetType = typeof(IMongoDbSet<>);

			foreach (var property in properties)
			{
				var propertyType = property.PropertyType;
				if (propertyType.IsGenericType && mongoDbSetType.IsAssignableFrom(propertyType))
				{
					yield return new DbSetInfo(property, propertyType.GetGenericArguments().FirstOrDefault());
				}
			}
		}
	}
}
