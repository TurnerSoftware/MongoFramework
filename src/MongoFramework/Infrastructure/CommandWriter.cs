using MongoDB.Driver;
using MongoFramework.Infrastructure.Commands;
using MongoFramework.Infrastructure.Diagnostics;
using MongoFramework.Infrastructure.Mapping;
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

		private IMongoCollection<TEntity> GetCollection() => Connection.GetDatabase().GetCollection<TEntity>(EntityDefinition.CollectionName);

		public void Write(IEnumerable<IWriteCommand<TEntity>> writeCommands)
		{
			var writeModel = writeCommands.SelectMany(c => c.GetModel());

			if (writeModel.Any())
			{
				using (var diagnostics = DiagnosticRunner.Start(Connection, writeModel))
				{
					try
					{
						GetCollection().BulkWrite(writeModel);
					}
					catch (Exception exception)
					{
						diagnostics.Error(exception);
						throw;
					}
				}
			}
		}

		public async Task WriteAsync(IEnumerable<IWriteCommand<TEntity>> writeCommands, CancellationToken cancellationToken = default(CancellationToken))
		{
			var writeModel = writeCommands.SelectMany(c => c.GetModel());

			cancellationToken.ThrowIfCancellationRequested();

			if (writeModel.Any())
			{
				using (var diagnostics = DiagnosticRunner.Start(Connection, writeModel))
				{
					try
					{
						await GetCollection().BulkWriteAsync(writeModel, null, cancellationToken).ConfigureAwait(false);
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
