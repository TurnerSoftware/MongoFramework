using MongoDB.Driver;
using MongoFramework.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MongoFramework.Infrastructure.Indexing
{
	public interface IIndexingProcessor
	{
		IEnumerable<CreateIndexModel<TEntity>> BuildIndexModel<TEntity>(IEnumerable<IEntityIndexMap> indexMapping);
	}
}
