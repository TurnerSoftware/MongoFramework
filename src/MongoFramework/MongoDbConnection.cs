﻿using System;
using System.Collections.Concurrent;
using MongoDB.Driver;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Diagnostics;

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

		public IDiagnosticListener DiagnosticListener { get; set; } = new NoOpDiagnosticListener();

		public static MongoDbConnection FromUrl(MongoUrl mongoUrl)
		{
			if (mongoUrl == null)
			{
				throw new ArgumentNullException(nameof(mongoUrl));
			}

			return new MongoDbConnection
			{
				Url = mongoUrl
			};
		}

		public static MongoDbConnection FromConnectionString(string connectionString) => FromUrl(new MongoUrl(connectionString));

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

		~MongoDbConnection() => Dispose(false);
	}
}
