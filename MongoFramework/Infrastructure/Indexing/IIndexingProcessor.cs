using System.Collections.Generic;
using MongoDB.Driver;

namespace MongoFramework.Infrastructure.Indexing
{
	public interface IIndexingProcessor
	{
		IEnumerable<CreateIndexModel<TEntity>> BuildIndexModel<TEntity>(IEnumerable<IEntityIndexMap> indexMapping);
	}
}
