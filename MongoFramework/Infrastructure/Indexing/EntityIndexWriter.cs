using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure.Indexing
{
	public class EntityIndexWriter<TEntity> : IEntityIndexWriter<TEntity>
	{
		private IMongoCollection<TEntity> Collection { get; set; }
		private IEntityIndexMapper IndexMapper { get; set; }

		public EntityIndexWriter(IMongoCollection<TEntity> collection, IEntityIndexMapper indexMapper)
		{
			Collection = collection;
			IndexMapper = indexMapper;
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

		public async Task ApplyIndexingAsync()
		{
			var indexModel = GenerateIndexModel();
			if (indexModel.Any())
			{
				await Collection.Indexes.CreateManyAsync(indexModel);
			}
		}
	}
}
