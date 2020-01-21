using MongoDB.Driver;
using MongoFramework.Infrastructure.Diagnostics;
using MongoFramework.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure.Indexing
{
	public class EntityIndexWriter<TEntity> : IEntityIndexWriter<TEntity> where TEntity : class
	{
		private IMongoDbConnection Connection { get; }
		private IEntityDefinition EntityDefinition { get; set; }
		public EntityIndexWriter(IMongoDbConnection connection)
		{
			Connection = connection;
			EntityDefinition = EntityMapping.GetOrCreateDefinition(typeof(TEntity));
		}

		private IMongoCollection<TEntity> GetCollection()
			=> Connection.GetDatabase().GetCollection<TEntity>(EntityDefinition.CollectionName);
		
		
		public void ApplyIndexing()
		{
			var indexModel = IndexModelBuilder<TEntity>.BuildModel().ToArray();
			if (indexModel.Length > 0)
			{
				using (var diagnostics = DiagnosticRunner.Start(Connection, indexModel))
				{
					try
					{
						GetCollection().Indexes.CreateMany(indexModel);
					}
					catch (Exception exception)
					{
						diagnostics.Error(exception);
						throw;
					}
				}
			}
		}

		public async Task ApplyIndexingAsync(CancellationToken cancellationToken = default)
		{
			var indexModel = IndexModelBuilder<TEntity>.BuildModel().ToArray();
			if (indexModel.Length > 0)
			{
				using (var diagnostics = DiagnosticRunner.Start(Connection, indexModel))
				{
					try
					{
						await GetCollection().Indexes.CreateManyAsync(indexModel, cancellationToken).ConfigureAwait(false);
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
