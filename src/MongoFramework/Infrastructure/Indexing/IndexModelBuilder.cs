using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Infrastructure.Indexing
{
	public static class IndexModelBuilder<TEntity>
	{
		public static IEnumerable<CreateIndexModel<TEntity>> BuildModel()
		{
			var indexBuilder = Builders<TEntity>.IndexKeys;
			var indexes = EntityMapping.GetOrCreateDefinition(typeof(TEntity)).Indexes;

			var groupedIndexModels = indexes
				.Select(d => new
				{
					d.IndexName,
					d.IndexPriority,
					IndexModel = CreateIndexModel(d)
				})
				.OrderBy(m => m.IndexPriority)
				.GroupBy(m => m.IndexName)
				.ToArray();

			foreach (var groupedModel in groupedIndexModels)
			{
				if (groupedModel.Key != null)
				{
					var keys = indexBuilder.Combine(groupedModel.Select(m => m.IndexModel.Keys));
					var options = groupedModel.FirstOrDefault().IndexModel.Options;
					yield return new CreateIndexModel<TEntity>(keys, options);
				}
				else
				{
					foreach (var model in groupedModel)
					{
						yield return model.IndexModel;
					}
				}
			}
		}

		private static CreateIndexModel<TEntity> CreateIndexModel(IEntityIndex indexDefinition)
		{
			var builder = Builders<TEntity>.IndexKeys;
			IndexKeysDefinition<TEntity> keyModel;

			if (indexDefinition.IndexType == IndexType.Text)
			{
				keyModel = builder.Text(indexDefinition.Property.FullPath);
			}
			else if (indexDefinition.IndexType == IndexType._2dSphere)
			{
				keyModel = builder.Geo2DSphere(indexDefinition.Property.FullPath);
			}
			else if (indexDefinition.IndexType == IndexType.GeoHaystack)
			{
				keyModel = builder.GeoHaystack(indexDefinition.Property.FullPath);
			}
			else
			{
				keyModel = indexDefinition.SortOrder == IndexSortOrder.Ascending ?
					builder.Ascending(indexDefinition.Property.FullPath) : builder.Descending(indexDefinition.Property.FullPath);
			}

			return new CreateIndexModel<TEntity>(keyModel, new CreateIndexOptions
			{
				Name = indexDefinition.IndexName,
				Unique = indexDefinition.IsUnique,
				Background = true
			});
		}
	}
}
