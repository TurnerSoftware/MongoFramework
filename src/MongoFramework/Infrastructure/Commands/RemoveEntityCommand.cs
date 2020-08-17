using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;
using MongoFramework.Infrastructure.DefinitionHelpers;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Infrastructure.Commands
{
	public class RemoveEntityCommand<TEntity> : IWriteCommand<TEntity> where TEntity : class
	{
		private EntityEntry EntityEntry { get; }

		public Type EntityType => typeof(TEntity);

		public RemoveEntityCommand(EntityEntry entityEntry)
		{
			EntityEntry = entityEntry;
		}

		public IEnumerable<WriteModel<TEntity>> GetModel()
		{
			var definition = EntityMapping.GetOrCreateDefinition(typeof(TEntity));
			yield return new DeleteOneModel<TEntity>(definition.CreateIdFilterFromEntity(EntityEntry.Entity as TEntity));
		}
	}
}
