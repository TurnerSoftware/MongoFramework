using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;

namespace MongoFramework.Infrastructure.EntityRelationships
{
	public interface IEntityNavigationCollection
	{
		IEnumerable<object> PersistingEntityIds { get; }
		void BeginImport(IEnumerable<object> entityIds);
		void FinaliseImport(IMongoDatabase database);
		void WriteChanges(IMongoDatabase database);
	}

	public interface IEntityNavigationCollection<TEntity> : IEntityNavigationCollection, IDbEntityCollection<TEntity>
	{

	}
}
