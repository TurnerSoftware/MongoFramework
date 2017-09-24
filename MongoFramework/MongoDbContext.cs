using System;
using System.Collections.Generic;
using MongoDB.Driver;
using System.Reflection;
using MongoFramework.Infrastructure;
using System.Linq;

namespace MongoFramework
{
	public class MongoDbContext : IMongoDbContext, IDisposable
	{
		protected IMongoDatabase database { get; set; }

		private IList<IMongoDbSet> dbSets { get; set; }

		public MongoDbContext(string connectionName)
		{
			var mongoUrl = MongoDbUtility.GetMongoUrlFromConfig(connectionName);

			if (mongoUrl == null)
			{
				throw new MongoConfigurationException("No connection string found with the name \'" + connectionName + "\'");
			}

			database = MongoDbUtility.GetDatabase(mongoUrl);
			InitialiseDbSets();
		}

		public MongoDbContext(string connectionString, string databaseName)
		{
			database = MongoDbUtility.GetDatabase(connectionString, databaseName);
			InitialiseDbSets();
		}

		internal MongoDbContext(IMongoDatabase database)
		{
			this.database = database;
			InitialiseDbSets();
		}
		public static MongoDbContext CreateWithDatabase(IMongoDatabase database)
		{
			return new MongoDbContext(database);
		}

		private void InitialiseDbSets()
		{
			dbSets = new List<IMongoDbSet>();

			//Construct the MongoDbSet properties
			var properties = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
			var mongoDbSetType = typeof(IMongoDbSet);
			foreach (var property in properties)
			{
				var propertyType = property.PropertyType;
				if (propertyType.IsGenericType && mongoDbSetType.IsAssignableFrom(propertyType))
				{
					var dbSet = Activator.CreateInstance(propertyType) as IMongoDbSet;
					dbSet.SetDatabase(database);
					dbSets.Add(dbSet);
					property.SetValue(this, dbSet);
				}
			}
		}
		
		public virtual void SaveChanges()
		{
			foreach (var dbSet in dbSets)
			{
				dbSet.SaveChanges();
			}
		}

		public void Dispose()
		{
			if (database != null)
			{
				database = null;
				dbSets = null;
			}
		}
	}
}
