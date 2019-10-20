using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure
{
	public interface IEntityRelationshipWriter<in TEntity> where TEntity : class
	{
		void CommitEntityRelationships(IEnumerable<TEntity> entities);
		Task CommitEntityRelationshipsAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
	}
}