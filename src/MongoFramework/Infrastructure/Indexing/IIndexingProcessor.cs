using MongoDB.Driver;
using System.Collections.Generic;

namespace MongoFramework.Infrastructure.Indexing
{
	public interface IIndexingProcessor
	{
		IEnumerable<CreateIndexModel<TEntity>> BuildIndexModel<TEntity>(IEnumerable<IEntityIndexMap> indexMapping) where TEntity : class;
	}
}
