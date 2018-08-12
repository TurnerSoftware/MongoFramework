using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure.EntityRelationships
{
	public interface IEntityRelationshipWriter<TEntity>
	{
		void CommitEntityRelationships(IEnumerable<TEntity> entities);
		Task CommitEntityRelationshipsAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
	}
}