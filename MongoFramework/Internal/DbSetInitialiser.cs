using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Settings;

namespace MongoFramework.Internal
{
	public class DbSetInitialiser
	{
		private IDbSetFinder DbSetFinder { get; }

		public DbSetInitialiser(IDbSetFinder dbSetFinder)
		{
			DbSetFinder = dbSetFinder;
		}

		public virtual DbSetCollection InitialiseSets(IMongoDbContext context, IDbContextSettings settings)
		{
			var result = new DbSetCollection();
			var services = new ServiceCollection();

			services.AddScoped(p => settings);

			foreach (var dbSetInfo in DbSetFinder.FindSets(context.GetType()))
			{
				var dbSet = Activator.CreateInstance(dbSetInfo.PropertyType, settings) as IMongoDbSet;
				dbSetInfo.Property.SetValue(context, dbSet);
				result.Add(dbSet);
			}

			return result;
		}
	}
}
