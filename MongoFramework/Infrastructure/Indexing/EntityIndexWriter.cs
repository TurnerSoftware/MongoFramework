using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure.Indexing
{
	public class EntityIndexWriter<TEntity> : IEntityIndexWriter<TEntity> where TEntity : class
	{
		private IMongoDbConnection Connection { get; }
		private IEntityIndexMapper IndexMapper { get; set; }

		public EntityIndexWriter(IMongoDbConnection connection)
		{
			Connection = connection;
			IndexMapper = connection.GetIndexMapper(typeof(TEntity));
		}

		private IMongoCollection<TEntity> GetCollection()
		{
			var collectionName = IndexMapper.GetCollectionName();
			return Connection.GetDatabase().GetCollection<TEntity>(collectionName);
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
				GetCollection().Indexes.CreateMany(indexModel);
			}
		}

		public async Task ApplyIndexingAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			var indexModel = GenerateIndexModel();
			if (indexModel.Any())
			{
				await GetCollection().Indexes.CreateManyAsync(indexModel, cancellationToken).ConfigureAwait(false);
			}
		}
	}
}
