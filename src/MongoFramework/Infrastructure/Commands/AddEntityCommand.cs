using System.Collections.Generic;
using MongoDB.Driver;

namespace MongoFramework.Infrastructure.Commands
{
	public class AddEntityCommand<TEntity> : IWriteCommand<TEntity> where TEntity : class
	{
		private EntityEntry<TEntity> EntityEntry { get; }
		public AddEntityCommand(EntityEntry<TEntity> entityEntry) => EntityEntry = entityEntry;
		public IEnumerable<WriteModel<TEntity>> GetModel()
		{
			yield return new InsertOneModel<TEntity>(EntityEntry.Entity);
		}
	}
}
