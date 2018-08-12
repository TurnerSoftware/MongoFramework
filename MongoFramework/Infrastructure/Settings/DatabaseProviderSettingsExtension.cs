using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;
using System.Linq;

namespace MongoFramework.Infrastructure.Settings
{
	public abstract class DatabaseProviderSettingsExtension : IDbContextSettingsExtension
	{
		public string ConnectionString { get; private set; }

		private MongoDatabaseSettings InternalDatabaseSettings;
		public MongoDatabaseSettings DatabaseSettings => InternalDatabaseSettings.Clone();

		protected DatabaseProviderSettingsExtension() { }
		protected DatabaseProviderSettingsExtension(DatabaseProviderSettingsExtension copyFrom)
		{
			ConnectionString = copyFrom.ConnectionString;
			InternalDatabaseSettings = copyFrom.InternalDatabaseSettings;
		}

		public abstract IMongoDatabase GetDatabase();

		public static DatabaseProviderSettingsExtension Extract(IDbContextSettings settings)
		{
			var providerExtensions = settings.Extensions.OfType<DatabaseProviderSettingsExtension>().ToList();

			if (providerExtensions.Count == 0)
			{
				throw new InvalidOperationException($"No database provider configured");
			}

			if (providerExtensions.Count > 1)
			{
				throw new InvalidOperationException($"Only one database provider can be configured");
			}

			return providerExtensions[0];
		}
	}
}
