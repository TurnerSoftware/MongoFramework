using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;

namespace MongoFramework.Infrastructure.Commands
{
	public interface IWriteCommand<TEntity> where TEntity : class
	{
		IEnumerable<WriteModel<TEntity>> GetModel();
	}
}
