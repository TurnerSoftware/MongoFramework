using System;
using System.Collections.Generic;
using MongoDB.Driver;

namespace MongoFramework.Infrastructure.Diagnostics
{
	public class DiagnosticCommand
	{
		public Guid CommandId { get; set; }
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
		public string Query { get; set; }
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
