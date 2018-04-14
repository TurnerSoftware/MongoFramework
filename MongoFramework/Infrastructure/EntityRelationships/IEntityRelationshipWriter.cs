using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace MongoFramework.Infrastructure.EntityRelationships
{
	public interface IEntityRelationshipWriter<TEntity>
	{
		IMongoDatabase Database { get; }
		void CommitEntityRelationships(IEnumerable<TEntity> entities);
		Task CommitEntityRelationshipsAsync(IEnumerable<TEntity> entities);
	}
}