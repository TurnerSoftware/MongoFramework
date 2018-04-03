using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;

namespace MongoFramework.Infrastructure.EntityRelationships
{
	public interface IEntityNavigationCollection
	{
		IEnumerable<string> ImportIds { get; }
		void BeginImport(IEnumerable<string> importIds);
		void FinaliseImport(IMongoDatabase database);
		void WriteChanges(IMongoDatabase database);
	}

	public interface IEntityNavigationCollection<TEntity> : IEntityNavigationCollection, IDbEntityCollection<TEntity>
	{

	}
}
