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
		{
			return Connection.GetDatabase().GetCollection<TEntity>(EntityDefinition.CollectionName);
		}
		
		public void ApplyIndexing()
		{
			var indexModel = IndexModelBuilder<TEntity>.BuildModel();
			if (indexModel.Any())
			{
				var commandId = Guid.NewGuid();
				try
				{
					Connection.DiagnosticListener.OnNext(new IndexDiagnosticCommand<TEntity>
					{
						CommandId = commandId,
						Source = $"{nameof(EntityIndexWriter<TEntity>)}.{nameof(ApplyIndexing)}",
						CommandState = CommandState.Start,
						IndexModel = indexModel
					});
					GetCollection().Indexes.CreateMany(indexModel);
					Connection.DiagnosticListener.OnNext(new IndexDiagnosticCommand<TEntity>
					{
						CommandId = commandId,
						Source = $"{nameof(EntityIndexWriter<TEntity>)}.{nameof(ApplyIndexing)}",
						CommandState = CommandState.End,
						IndexModel = indexModel
					});
				}
				catch (Exception ex)
				{
					Connection.DiagnosticListener.OnNext(new IndexDiagnosticCommand<TEntity>
					{
						CommandId = commandId,
						Source = $"{nameof(EntityIndexWriter<TEntity>)}.{nameof(ApplyIndexing)}",
						CommandState = CommandState.Error,
						IndexModel = indexModel
					});
					Connection.DiagnosticListener.OnError(ex);

					throw;
				}
			}
		}

		public async Task ApplyIndexingAsync(CancellationToken cancellationToken = default)
		{
			var indexModel = IndexModelBuilder<TEntity>.BuildModel();
			if (indexModel.Any())
			{
				var commandId = Guid.NewGuid();
				try
				{
					Connection.DiagnosticListener.OnNext(new IndexDiagnosticCommand<TEntity>
					{
						CommandId = commandId,
						Source = $"{nameof(EntityIndexWriter<TEntity>)}.{nameof(ApplyIndexingAsync)}",
						CommandState = CommandState.Start,
						IndexModel = indexModel
					});
					await GetCollection().Indexes.CreateManyAsync(indexModel, cancellationToken).ConfigureAwait(false);
					Connection.DiagnosticListener.OnNext(new IndexDiagnosticCommand<TEntity>
					{
						CommandId = commandId,
						Source = $"{nameof(EntityIndexWriter<TEntity>)}.{nameof(ApplyIndexingAsync)}",
						CommandState = CommandState.End,
						IndexModel = indexModel
					});
				}
				catch (Exception ex)
				{
					Connection.DiagnosticListener.OnNext(new IndexDiagnosticCommand<TEntity>
					{
						CommandId = commandId,
						Source = $"{nameof(EntityIndexWriter<TEntity>)}.{nameof(ApplyIndexingAsync)}",
						CommandState = CommandState.Error,
						IndexModel = indexModel
					});
					Connection.DiagnosticListener.OnError(ex);

					throw;
				}
			}
		}
	}
}
