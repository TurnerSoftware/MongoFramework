using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Commands;

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
	}
}
