using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using MongoFramework.Attributes;
using System.Threading.Tasks;
using System.Collections.Concurrent;

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
			Collection.Indexes.CreateMany(indexModel);
		}
		
		public async Task ApplyIndexingAsync()
		{
			var indexModel = GenerateIndexModel();
			await Collection.Indexes.CreateManyAsync(indexModel);
		}
	}
}
