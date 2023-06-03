using System;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Diagnostics;
using MongoFramework.Utilities;

namespace MongoFramework
{
	public class MongoDbConnection : IMongoDbConnection
	{
		public MongoUrl Url { get; protected set; }
		private bool IsDisposed { get; set; }

		private Action<MongoClientSettings> ConfigureSettings { get; init; }

		private IMongoClient InternalClient;
		public IMongoClient Client
		{
			get
			{
				if (IsDisposed)
				{
					throw new ObjectDisposedException(nameof(MongoDbConnection));
				}

				if (InternalClient == null)
				{
					var settings = MongoClientSettings.FromUrl(Url);
					ConfigureSettings?.Invoke(settings);
					settings.LinqProvider = MongoDB.Driver.Linq.LinqProvider.V2;
					InternalClient = new MongoClient(settings);
				}

				return InternalClient;
			}
		}

		public IDiagnosticListener DiagnosticListener { get; set; } = new NoOpDiagnosticListener();

		public static MongoDbConnection FromUrl(MongoUrl mongoUrl) => FromUrl(mongoUrl, configureSettings: null);
		public static MongoDbConnection FromUrl(MongoUrl mongoUrl, Action<MongoClientSettings> configureSettings)
		{
			Check.NotNull(mongoUrl, nameof(mongoUrl));

			return new MongoDbConnection
			{
				Url = mongoUrl,
				ConfigureSettings = configureSettings
			};
		}

		public static MongoDbConnection FromConnectionString(string connectionString) => FromConnectionString(connectionString, configureSettings: null);
		public static MongoDbConnection FromConnectionString(string connectionString, Action<MongoClientSettings> configureSettings) => FromUrl(new MongoUrl(connectionString), configureSettings);

		public IMongoDatabase GetDatabase()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(nameof(MongoDbConnection));
			}

			return Client.GetDatabase(Url.DatabaseName);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (IsDisposed)
			{
				return;
			}

			if (disposing)
			{
				InternalClient = null;
				IsDisposed = true;
			}
		}

		~MongoDbConnection()
		{
			Dispose(false);
		}
	}
}
