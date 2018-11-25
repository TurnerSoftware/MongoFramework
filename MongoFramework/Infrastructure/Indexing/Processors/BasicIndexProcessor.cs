using MongoDB.Driver;
using MongoFramework.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace MongoFramework.Infrastructure.Indexing.Processors
{
	public class BasicIndexProcessor : IIndexingProcessor
	{
		public IEnumerable<CreateIndexModel<TEntity>> BuildIndexModel<TEntity>(IEnumerable<IEntityIndexMap> indexMapping) where TEntity : class
		{
			var result = new List<CreateIndexModel<TEntity>>();
			var indexBuilder = Builders<TEntity>.IndexKeys;

			var groupedIndexModels = indexMapping
				.Where(m => m.Index is IndexAttribute)
				.Select(m => new
				{
					m.Index.Name,
					(m.Index as IndexAttribute).IndexPriority,
					IndexModel = GetBaseIndexModel<TEntity>(m)
				})
				.OrderBy(m => m.IndexPriority)
				.GroupBy(m => m.Name)
				.ToArray();

			foreach (var groupedModel in groupedIndexModels)
			{
				if (groupedModel.Key != null)
				{
					var keys = indexBuilder.Combine(groupedModel.Select(m => m.IndexModel.Keys));
					var options = groupedModel.FirstOrDefault().IndexModel.Options;
					result.Add(new CreateIndexModel<TEntity>(keys, options));
				}
				else
				{
					result.AddRange(groupedModel.Select(m => m.IndexModel));
				}
			}

			return result;
		}

		private CreateIndexModel<TEntity> GetBaseIndexModel<TEntity>(IEntityIndexMap indexMap)
		{
			var index = indexMap.Index;
			var builder = Builders<TEntity>.IndexKeys;
			var indexDefinition = index.SortOrder == IndexSortOrder.Ascending ? builder.Ascending(indexMap.FullPath) : builder.Descending(indexMap.FullPath);

			return new CreateIndexModel<TEntity>(indexDefinition, new CreateIndexOptions
			{
				Name = index.Name,
				Unique = index.IsUnique,
				Background = true
			});
		}
	}
}
