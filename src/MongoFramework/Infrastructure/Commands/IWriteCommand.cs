using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;

namespace MongoFramework.Infrastructure.Commands
{
	public interface IWriteCommand
	{
		Type EntityType { get; }
	}

	public interface IWriteCommand<TEntity> : IWriteCommand where TEntity : class
	{
		IEnumerable<WriteModel<TEntity>> GetModel();
	}
}
