using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;

namespace MongoFramework.Infrastructure.Commands
{
	public class AddEntityCommand<TEntity> : IWriteCommand<TEntity> where TEntity : class
	{
		private EntityEntry EntityEntry { get; }

		public AddEntityCommand(EntityEntry entityEntry)
		{
			EntityEntry = entityEntry;
		}

		public IEnumerable<WriteModel<TEntity>> GetModel()
		{
			yield return new InsertOneModel<TEntity>(EntityEntry.Entity as TEntity);
		}
	}
}
