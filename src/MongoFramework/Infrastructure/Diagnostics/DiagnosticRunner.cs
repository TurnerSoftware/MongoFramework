using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Linq;

namespace MongoFramework.Infrastructure.Diagnostics
{
	public class DiagnosticRunner : IDisposable
	{
		public Guid CommandId { get; } = Guid.NewGuid();
		public IMongoDbConnection Connection { get; }
		public bool HasErrored { get; private set; }

		private DiagnosticRunner(IMongoDbConnection connection)
		{
			Connection = connection;
		}
		public static DiagnosticRunner Start<TEntity, TOutput>(IMongoDbConnection connection, IMongoFrameworkQueryProvider<TEntity, TOutput> provider) where TEntity : class
		{
			var runner = new DiagnosticRunner(connection);
			connection.DiagnosticListener.OnNext(new ReadDiagnosticCommand
			{
				CommandId = runner.CommandId,
				CommandState = CommandState.Start,
				EntityType = typeof(TOutput),
				Query = provider.ToQuery()
			});
			return runner;
		}
		public static DiagnosticRunner Start<TEntity>(IMongoDbConnection connection, IEnumerable<WriteModel<TEntity>> model) where TEntity : class
		{
			var runner = new DiagnosticRunner(connection);
			connection.DiagnosticListener.OnNext(new WriteDiagnosticCommand<TEntity>
			{
				CommandId = runner.CommandId,
				CommandState = CommandState.Start,
				EntityType = typeof(TEntity),
				WriteModel = model
			});
			return runner;
		}
		public static DiagnosticRunner Start<TEntity>(IMongoDbConnection connection, IEnumerable<CreateIndexModel<TEntity>> model) where TEntity : class
		{
			var runner = new DiagnosticRunner(connection);
			connection.DiagnosticListener.OnNext(new IndexDiagnosticCommand<TEntity>
			{
				CommandId = runner.CommandId,
				CommandState = CommandState.Start,
				EntityType = typeof(TEntity),
				IndexModel = model
			});
			return runner;
		}

		public void FirstReadResult<TOutput>()
		{
			Connection.DiagnosticListener.OnNext(new ReadDiagnosticCommand
			{
				CommandId = CommandId,
				CommandState = CommandState.FirstResult,
				EntityType = typeof(TOutput)
			});
		}

		public void Error(Exception exception = null)
		{
			HasErrored = true;
			Connection.DiagnosticListener.OnNext(new DiagnosticCommand
			{
				CommandId = CommandId,
				CommandState = CommandState.Error
			});

			if (exception != null)
			{
				Connection.DiagnosticListener.OnError(exception);
			}
		}

		public void Dispose()
		{
			if (!HasErrored)
			{
				Connection.DiagnosticListener.OnNext(new DiagnosticCommand
				{
					CommandId = CommandId,
					CommandState = CommandState.End
				});
			}
		}
	}
}
