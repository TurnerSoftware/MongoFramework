using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoFramework.Infrastructure.Diagnostics;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Infrastructure.Indexing
{
	public static class EntityIndexWriter
	{
		private static readonly ConcurrentDictionary<Type, bool> HasAppliedIndexes = new ConcurrentDictionary<Type, bool>();

		public static void ClearCache()
		{
			HasAppliedIndexes.Clear();
		}

		public static void ApplyIndexing<TEntity>(IMongoDbConnection connection) where TEntity : class
		{
			if (HasAppliedIndexes.TryGetValue(typeof(TEntity), out var hasApplied) && hasApplied)
			{
				return;
			}

			var indexModel = IndexModelBuilder<TEntity>.BuildModel().ToArray();
			if (indexModel.Length > 0)
			{
				var definition = EntityMapping.GetOrCreateDefinition(typeof(TEntity));
				using (var diagnostics = DiagnosticRunner.Start(connection, indexModel))
				{
					try
					{
						var collection = connection.GetDatabase().GetCollection<TEntity>(definition.CollectionName);
						collection.Indexes.CreateMany(indexModel);
						HasAppliedIndexes.TryAdd(typeof(TEntity), true);
					}
					catch (Exception exception)
					{
						diagnostics.Error(exception);
						throw;
					}
				}
			}
		}

		public static async Task ApplyIndexingAsync<TEntity>(IMongoDbConnection connection, CancellationToken cancellationToken = default) where TEntity : class
		{
			if (HasAppliedIndexes.TryGetValue(typeof(TEntity), out var hasApplied) && hasApplied)
			{
				return;
			}

			var indexModel = IndexModelBuilder<TEntity>.BuildModel().ToArray();
			if (indexModel.Length > 0)
			{
				var definition = EntityMapping.GetOrCreateDefinition(typeof(TEntity));
				using (var diagnostics = DiagnosticRunner.Start(connection, indexModel))
				{
					try
					{
						var collection = connection.GetDatabase().GetCollection<TEntity>(definition.CollectionName);
						await collection.Indexes.CreateManyAsync(indexModel, cancellationToken).ConfigureAwait(false);
						HasAppliedIndexes.TryAdd(typeof(TEntity), true);
					}
					catch (Exception exception)
					{
						diagnostics.Error(exception);
						throw;
					}
				}
			}
		}
	}
}
