using MongoDB.Driver;
using MongoFramework.Infrastructure.Commands;
using MongoFramework.Infrastructure.DefinitionHelpers;
using MongoFramework.Infrastructure.Diagnostics;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mutation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure
{
	public class CommandWriter<TEntity> : ICommandWriter<TEntity> where TEntity : class
	{
		public IMongoDbConnection Connection { get; }
		private IEntityDefinition EntityDefinition { get; }
		
		public CommandWriter(IMongoDbConnection connection)
		{
			Connection = connection ?? throw new ArgumentNullException(nameof(connection));
			EntityDefinition = EntityMapping.GetOrCreateDefinition(typeof(TEntity));
		}

		private IMongoCollection<TEntity> GetCollection()
		{
			return Connection.GetDatabase().GetCollection<TEntity>(EntityDefinition.CollectionName);
		}

		public void Write(IEnumerable<IWriteCommand<TEntity>> writeCommands)
		{
			var writeModel = writeCommands.SelectMany(c => c.GetModel());

			if (writeModel.Any())
			{
				var commandId = Guid.NewGuid();
				try
				{
					Connection.DiagnosticListener.OnNext(new WriteDiagnosticCommand<TEntity>
					{
						CommandId = commandId,
						Source = $"{nameof(CommandWriter<TEntity>)}.{nameof(Write)}",
						CommandState = CommandState.Start,
						EntityType = typeof(TEntity),
						WriteModel = writeModel
					});
					GetCollection().BulkWrite(writeModel);
					Connection.DiagnosticListener.OnNext(new WriteDiagnosticCommand<TEntity>
					{
						CommandId = commandId,
						Source = $"{nameof(CommandWriter<TEntity>)}.{nameof(Write)}",
						CommandState = CommandState.End,
						EntityType = typeof(TEntity),
						WriteModel = writeModel
					});
				}
				catch (Exception ex)
				{
					Connection.DiagnosticListener.OnNext(new WriteDiagnosticCommand<TEntity>
					{
						CommandId = commandId,
						Source = $"{nameof(CommandWriter<TEntity>)}.{nameof(Write)}",
						CommandState = CommandState.Error,
						EntityType = typeof(TEntity),
						WriteModel = writeModel
					});
					Connection.DiagnosticListener.OnError(ex);

					throw;
				}
			}
		}

		public async Task WriteAsync(IEnumerable<IWriteCommand<TEntity>> writeCommands, CancellationToken cancellationToken = default(CancellationToken))
		{
			var writeModel = writeCommands.SelectMany(c => c.GetModel());

			cancellationToken.ThrowIfCancellationRequested();

			if (writeModel.Any())
			{
				var commandId = Guid.NewGuid();
				try
				{
					Connection.DiagnosticListener.OnNext(new WriteDiagnosticCommand<TEntity>
					{
						CommandId = commandId,
						Source = $"{nameof(CommandWriter<TEntity>)}.{nameof(WriteAsync)}",
						CommandState = CommandState.Start,
						EntityType = typeof(TEntity),
						WriteModel = writeModel
					});
					await GetCollection().BulkWriteAsync(writeModel, null, cancellationToken).ConfigureAwait(false);
					Connection.DiagnosticListener.OnNext(new WriteDiagnosticCommand<TEntity>
					{
						CommandId = commandId,
						Source = $"{nameof(CommandWriter<TEntity>)}.{nameof(WriteAsync)}",
						CommandState = CommandState.End,
						EntityType = typeof(TEntity),
						WriteModel = writeModel
					});
				}
				catch (Exception ex)
				{
					Connection.DiagnosticListener.OnNext(new WriteDiagnosticCommand<TEntity>
					{
						CommandId = commandId,
						Source = $"{nameof(CommandWriter<TEntity>)}.{nameof(WriteAsync)}",
						CommandState = CommandState.Error,
						EntityType = typeof(TEntity),
						WriteModel = writeModel
					});
					Connection.DiagnosticListener.OnError(ex);

					throw;
				}
			}
		}
	}
}
