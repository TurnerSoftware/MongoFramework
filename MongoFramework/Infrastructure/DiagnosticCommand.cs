using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Linq;

namespace MongoFramework.Infrastructure
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
		End,
		Error
	}

	public class ReadDiagnosticCommand : DiagnosticCommand
	{
		public IMongoFrameworkQueryable Queryable { get; set; }
	}

	public class WriteDiagnosticCommand<TEntity> : DiagnosticCommand
	{
		public IEnumerable<WriteModel<TEntity>> WriteModel { get; set; }
	}

	public class IndexDiagnosticCommand<TEntity> : DiagnosticCommand
	{
		public IEnumerable<CreateIndexModel<TEntity>> IndexModel { get; set; }
	}
}
