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
			var writeModel = writeCommands.SelectMany(c => c.GetModel()).ToArray();

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
			var writeModel = writeCommands.SelectMany(c => c.GetModel()).ToArray();

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
