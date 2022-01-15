using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoFramework.Infrastructure;

namespace MongoFramework
{
	public interface IMongoDbContext
	{
		IMongoDbConnection Connection { get; }

		EntityEntryContainer ChangeTracker { get; }
		EntityCommandStaging CommandStaging { get; }

		IMongoDbSet<TEntity> Set<TEntity>() where TEntity : class;
		IQueryable<TEntity> Query<TEntity>() where TEntity : class;

		void SaveChanges();
		Task SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));

		void Attach<TEntity>(TEntity entity) where TEntity : class;
		void AttachRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
	}
}
