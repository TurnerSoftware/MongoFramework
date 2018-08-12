using MongoFramework.Infrastructure;
using MongoFramework.Internal;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MongoFramework
{
	public class MongoDbContext : IMongoDbContext, IDisposable
	{
		private DbSetCollection DbSetCollection { get; set; }

		public MongoDbContext() : this(new DbContextSettings()) { }

		public MongoDbContext(IDbContextSettings settings)
		{
			if (settings == null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			var settingsBuilder = new DbContextSettingsBuilder(settings);
			OnConfiguring(settingsBuilder);

			var dbSetInitialiser = new DbSetInitialiser(new DbSetFinder());
			DbSetCollection = dbSetInitialiser.InitialiseSets(this, settingsBuilder.Settings);
		}

		protected internal virtual void OnConfiguring(DbContextSettingsBuilder settingsBuilder) { }

		public virtual void SaveChanges() => DbSetCollection.SaveChanges();

		public virtual async Task SaveChangesAsync(CancellationToken cancellationToken = default) => await DbSetCollection.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				DbSetCollection = null;
			}
		}

		~MongoDbContext()
		{
			Dispose(false);
		}
	}
}
