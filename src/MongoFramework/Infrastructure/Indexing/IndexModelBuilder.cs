using System;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Infrastructure.Indexing;

public static class IndexModelBuilder<TEntity>
{
	public static IEnumerable<CreateIndexModel<TEntity>> BuildModel()
	{
		var indexBuilder = Builders<TEntity>.IndexKeys;
		var indexes = EntityMapping.GetOrCreateDefinition(typeof(TEntity)).Indexes;

		foreach (var index in indexes)
		{
			var indexKeyCount = index.IndexPaths.Count + (index.IsTenantExclusive ? 1 : 0);
			var indexKeys = new IndexKeysDefinition<TEntity>[indexKeyCount];
			for (var i = 0; i < index.IndexPaths.Count; i++)
			{
				indexKeys[i] = CreateIndexKey(index.IndexPaths[i]);
			}
			
			if (index.IsTenantExclusive)
			{
				indexKeys[indexKeys.Length - 1] = Builders<TEntity>.IndexKeys.Ascending(nameof(IHaveTenantId.TenantId));
			}

			var combinedKeyDefinition = indexBuilder.Combine(indexKeys);
			yield return new CreateIndexModel<TEntity>(combinedKeyDefinition, new CreateIndexOptions
			{
				Name = index.IndexName,
				Unique = index.IsUnique,
				Background = true
			});
		}
	}

	private static IndexKeysDefinition<TEntity> CreateIndexKey(IndexPathDefinition indexPathDefinition)
	{
		var builder = Builders<TEntity>.IndexKeys;
		Func<FieldDefinition<TEntity>, IndexKeysDefinition<TEntity>> builderMethod = indexPathDefinition.IndexType switch
		{
			IndexType.Standard when indexPathDefinition.SortOrder == IndexSortOrder.Ascending => builder.Ascending,
			IndexType.Standard when indexPathDefinition.SortOrder == IndexSortOrder.Descending => builder.Descending,
			IndexType.Text => builder.Text,
			IndexType.Geo2dSphere => builder.Geo2DSphere,
			_ => throw new ArgumentException($"Unsupported index type \"{indexPathDefinition.IndexType}\"", nameof(indexPathDefinition))
		};
		return builderMethod(indexPathDefinition.Path);
	}
}
