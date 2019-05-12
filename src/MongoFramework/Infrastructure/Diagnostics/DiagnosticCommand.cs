using System;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Linq;

namespace MongoFramework.Infrastructure.Diagnostics
{
	public abstract class DiagnosticCommand
	{
		public Guid CommandId { get; set; }
		public string Source { get; set; }
		public CommandState CommandState { get; set; }
		public Type EntityType { get; set; }
	}

	public enum CommandState
	{
		Start,
		FirstResult,
		End,
		Error
	}

	public class ReadDiagnosticCommand : DiagnosticCommand
	{
		public IMongoFrameworkQueryable Queryable { get; set; }
	}

	public abstract class WriteDiagnosticCommandBase : DiagnosticCommand { }
	public class WriteDiagnosticCommand<TEntity> : WriteDiagnosticCommandBase
	{
		public IEnumerable<WriteModel<TEntity>> WriteModel { get; set; }
	}

	public abstract class IndexDiagnosticCommandBase : DiagnosticCommand { }
	public class IndexDiagnosticCommand<TEntity> : IndexDiagnosticCommandBase
	{
		public IEnumerable<CreateIndexModel<TEntity>> IndexModel { get; set; }
	}
}
