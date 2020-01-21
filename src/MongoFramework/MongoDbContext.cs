using MongoFramework.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MongoFramework
{
	public class MongoDbContext : IMongoDbContext, IDisposable
	{
		protected IMongoDbConnection Connection { get; private set; }
		private IList<IMongoDbSet> DbSets { get; set; }
		public MongoDbContext(IMongoDbConnection connection)
		{
			Connection = connection;
			InitialiseDbSets();
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
					var dbSet = OnDbSetCreation(property);
					DbSets.Add(dbSet);
					property.SetValue(this, dbSet);
				}
			}
		}

		protected virtual IMongoDbSet OnDbSetCreation(PropertyInfo property)
		{
			IMongoDbSet dbSet;

			var propertyType = property.PropertyType;
			var dbSetWithOptionsConstructor = propertyType.GetConstructor(new[] { typeof(IDbSetOptions) });
			if (dbSetWithOptionsConstructor != null)
			{
				var dbSetOptionsAttribute = property.GetCustomAttribute<DbSetOptionsAttribute>();
				var dbSetOptions = dbSetOptionsAttribute?.GetOptions();
				dbSet = dbSetWithOptionsConstructor.Invoke(new[] { dbSetOptions }) as IMongoDbSet;
			}
			else
			{
				dbSet = Activator.CreateInstance(propertyType) as IMongoDbSet;
			}

			dbSet.SetConnection(Connection);
			return dbSet;
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
				Connection?.Dispose();
				Connection = null;
				DbSets = null;
			}
		}

		~MongoDbContext() => Dispose(false);
	}
}
