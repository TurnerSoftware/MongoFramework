using System.Collections.Generic;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Infrastructure.Commands
{
	public class RemoveEntityCommand<TEntity> : IWriteCommand<TEntity> where TEntity : class
	{
		private EntityEntry<TEntity> EntityEntry { get; }
		public RemoveEntityCommand(EntityEntry<TEntity> entityEntry) => EntityEntry = entityEntry;

		public IEnumerable<WriteModel<TEntity>> GetModel()
		{
			var definition = EntityMapping.GetOrCreateDefinition(typeof(TEntity));
			yield return new DeleteOneModel<TEntity>(definition.CreateIdFilterFromEntity(EntityEntry.Entity));
		}
	}
}
