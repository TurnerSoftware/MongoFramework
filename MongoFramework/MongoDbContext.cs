using MongoDB.Driver;
using MongoFramework.Infrastructure;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MongoFramework
{
	public class MongoDbContext : IMongoDbContext, IDisposable
	{
		protected IMongoDatabase Database { get; set; }

		private IList<IMongoDbSet> DbSets { get; set; }

#if !NETCOREAPP2_0
		public MongoDbContext(string connectionName)
		{
			var mongoUrl = MongoDbUtility.GetMongoUrlFromConfig(connectionName);

			if (mongoUrl == null)
			{
				throw new MongoConfigurationException("No connection string found with the name \'" + connectionName + "\'");
			}

			Database = MongoDbUtility.GetDatabase(mongoUrl);
			InitialiseDbSets();
		}
#endif
		public MongoDbContext(string connectionString, string databaseName)
		{
			Database = MongoDbUtility.GetDatabase(connectionString, databaseName);
			InitialiseDbSets();
		}

		public MongoDbContext(IMongoDbContextOptions options)
		{
			Database = MongoDbUtility.GetDatabase(options);
			InitialiseDbSets();
		}

		internal MongoDbContext(IMongoDatabase database)
		{
			Database = database;
			InitialiseDbSets();
		}
		public static MongoDbContext CreateWithDatabase(IMongoDatabase database)
		{
			return new MongoDbContext(database);
		}

		private void InitialiseDbSets()
		{
			DbSets = new List<IMongoDbSet>();

			//Construct the MongoDbSet properties
			var properties = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
			var mongoDbSetType = typeof(IMongoDbSet);
			foreach (var property in properties)
			{
				var propertyType = property.PropertyType;
				if (propertyType.IsGenericType && mongoDbSetType.IsAssignableFrom(propertyType))
				{
					var dbSet = Activator.CreateInstance(propertyType) as IMongoDbSet;
					dbSet.SetDatabase(Database);
					DbSets.Add(dbSet);
					property.SetValue(this, dbSet);
				}
			}
		}

		public virtual void SaveChanges()
		{
			foreach (var dbSet in DbSets)
			{
				dbSet.SaveChanges();
			}
		}

		public virtual async Task SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			foreach (var dbSet in DbSets)
			{
				await dbSet.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				Database = null;
				DbSets = null;
			}
		}

		~MongoDbContext()
		{
			Dispose(false);
		}
	}
}
