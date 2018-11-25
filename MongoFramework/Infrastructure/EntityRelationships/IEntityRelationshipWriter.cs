using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure.EntityRelationships
{
	public interface IEntityRelationshipWriter<TEntity> where TEntity : class
	{
		IMongoDatabase Database { get; }
		void CommitEntityRelationships(IEnumerable<TEntity> entities);
		Task CommitEntityRelationshipsAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default(CancellationToken));
	}
}