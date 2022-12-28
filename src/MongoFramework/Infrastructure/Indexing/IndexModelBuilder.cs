﻿using System.Collections.Generic;
using System.Linq;
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
			var groupedIndexes = indexes.OrderBy(i => i.IndexPriority).GroupBy(i => i.IndexName);

			foreach (var indexGroup in groupedIndexes)
			{
				if (indexGroup.Key != null)
				{
					var indexKeys = new List<IndexKeysDefinition<TEntity>>();
					CreateIndexOptions<TEntity> indexOptions = default;
					foreach (var index in indexGroup)
					{
						var indexModel = CreateIndexModel(index);
						indexKeys.Add(indexModel.Keys);

						if (indexOptions == null)
						{
							indexOptions = indexModel.Options;
						}
					}

					var combinedKeyDefinition = indexBuilder.Combine(indexKeys);
					yield return new CreateIndexModel<TEntity>(combinedKeyDefinition, indexOptions);
				}
				else
				{
					foreach (var index in indexGroup)
					{
						yield return CreateIndexModel(index);
					}
				}
			}
		}

		private static CreateIndexModel<TEntity> CreateIndexModel(IEntityIndexDefinition indexDefinition)
		{
			var builder = Builders<TEntity>.IndexKeys;
			IndexKeysDefinition<TEntity> keyModel;

			if (indexDefinition.IndexType == IndexType.Text)
			{
				keyModel = builder.Text(indexDefinition.Path);
			}
			else if (indexDefinition.IndexType == IndexType.Geo2dSphere)
			{
				keyModel = builder.Geo2DSphere(indexDefinition.Path);
			}
			else
			{
				keyModel = indexDefinition.SortOrder == IndexSortOrder.Ascending ?
					builder.Ascending(indexDefinition.Path) : builder.Descending(indexDefinition.Path);
			}

			if (indexDefinition.IsTenantExclusive && typeof(IHaveTenantId).IsAssignableFrom(typeof(TEntity)))
			{
				var tenantKey = indexDefinition.SortOrder == IndexSortOrder.Ascending ?
					builder.Ascending("TenantId") : builder.Descending("TenantId");
				keyModel = builder.Combine(tenantKey, keyModel);
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
