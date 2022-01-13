using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Driver;
using MongoFramework.Infrastructure.DefinitionHelpers;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Infrastructure.Commands
{
	public class UpdateEntityCommand<TEntity> : IWriteCommand<TEntity> where TEntity : class
	{
		private EntityEntry EntityEntry { get; }

		public Type EntityType => typeof(TEntity);

		public UpdateEntityCommand(EntityEntry entityEntry)
		{
			EntityEntry = entityEntry;
		}

		public IEnumerable<WriteModel<TEntity>> GetModel(WriteModelOptions options)
		{
			var entity = EntityEntry.Entity as TEntity;

			var validationContext = new ValidationContext(entity);
			Validator.ValidateObject(entity, validationContext);

			var definition = EntityMapping.GetOrCreateDefinition(typeof(TEntity));
			var updateDefinition = UpdateDefinitionHelper.CreateFromDiff<TEntity>(EntityEntry.OriginalValues, EntityEntry.CurrentValues);
			yield return new UpdateOneModel<TEntity>(definition.CreateIdFilterFromEntity(entity), updateDefinition);
		}
	}
}
