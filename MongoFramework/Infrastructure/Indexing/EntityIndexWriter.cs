using MongoDB.Driver;
using MongoFramework.Infrastructure.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure.Indexing
{
	public class EntityIndexWriter<TEntity> : IEntityIndexWriter<TEntity>
	{
		private IMongoCollection<TEntity> Collection { get; set; }
		private IEntityIndexMapper IndexMapper { get; set; }

		public EntityIndexWriter(IDbContextSettings settings)
		{
			var database = settings.GetDatabase();
			var extensionSettings = EntityMappingProviderSettingsExtension.Extract(settings);
			var entityMapper = extensionSettings.GetEntityMapper(typeof(TEntity));

			Collection = database.GetCollection<TEntity>(entityMapper.GetCollectionName());
			IndexMapper = extensionSettings.GetEntityIndexMapper(typeof(TEntity));
		}

		private IEnumerable<CreateIndexModel<TEntity>> GenerateIndexModel()
		{
			var indexMapping = IndexMapper.GetIndexMapping();
			var processors = DefaultIndexingPack.Instance.Processors;
			return processors.SelectMany(d => d.BuildIndexModel<TEntity>(indexMapping));
		}

		public void ApplyIndexing()
		{
			var indexModel = GenerateIndexModel();
			if (indexModel.Any())
			{
				Collection.Indexes.CreateMany(indexModel);
			}
		}

		public async Task ApplyIndexingAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			var indexModel = GenerateIndexModel();
			if (indexModel.Any())
			{
				await Collection.Indexes.CreateManyAsync(indexModel, cancellationToken).ConfigureAwait(false);
			}
		}
	}
}
