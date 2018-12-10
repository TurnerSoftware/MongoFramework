using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Indexing;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework
{
	public class MongoDbConnection : IMongoDbConnection
	{
		public MongoUrl Url { get; private set; }
		private bool IsDisposed { get; set; }

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
					InternalClient = new MongoClient(Url);
				}

				return InternalClient;
			}
		}

		private ConcurrentDictionary<Type, IEntityMapper> EntityMapperCache { get; } = new ConcurrentDictionary<Type, IEntityMapper>();

		public IDiagnosticListener DiagnosticListener { get; set; } = new NoOpDiagnosticListener();

		public static MongoDbConnection FromUrl(MongoUrl mongoUrl)
		{
			if (mongoUrl == null)
			{
				throw new ArgumentNullException(nameof(mongoUrl));
			}

			return new MongoDbConnection()
			{
				Url = mongoUrl
			};
		}

		public static MongoDbConnection FromConnectionString(string connectionString, string databaseName)
		{
			var urlBuilder = new MongoUrlBuilder(connectionString)
			{
				DatabaseName = databaseName
			};

			return FromUrl(urlBuilder.ToMongoUrl());
		}

#if !NETCOREAPP2_0
		public static MongoDbConnection FromConfig(string connectionName)
		{
			var connectionStringConfig = System.Configuration.ConfigurationManager.ConnectionStrings[connectionName];

			if (connectionStringConfig != null)
			{
				var mongoUrl = MongoUrl.Create(connectionStringConfig.ConnectionString);
				return FromUrl(mongoUrl);
			}

			return null;
		}
#endif

		public IMongoDatabase GetDatabase()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(nameof(MongoDbConnection));
			}

			return Client.GetDatabase(Url.DatabaseName);
		}

		public IEntityMapper GetEntityMapper(Type entityType)
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(nameof(MongoDbConnection));
			}

			return EntityMapperCache.GetOrAdd(entityType, (type) =>
			{
				return new EntityMapper(type, this);
			});
		}

		public IEntityIndexMapper GetIndexMapper(Type entityType)
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(nameof(MongoDbConnection));
			}

			var entityMapper = GetEntityMapper(entityType);
			return new EntityIndexMapper(entityMapper);
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
