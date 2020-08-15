using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Driver;
using MongoFramework.Bson;
using MongoFramework.Infrastructure.DefinitionHelpers;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mutation;

namespace MongoFramework.Infrastructure.Commands
{
	public class UpdateEntityCommand<TEntity> : IWriteCommand<TEntity> where TEntity : class
	{
		private EntityEntry EntityEntry { get; }

		public UpdateEntityCommand(EntityEntry entityEntry)
		{
			EntityEntry = entityEntry;
		}

		public IEnumerable<WriteModel<TEntity>> GetModel()
		{
			var entity = EntityEntry.Entity as TEntity;
			EntityMutation<TEntity>.MutateEntity(entity, MutatorType.Update);

			//MongoDB doesn't like it if an UpdateDefinition is empty.
			//This is primarily to work around a mutation that may set an entity to its default state.
			if (BsonDiff.HasDifferences(EntityEntry.OriginalValues, EntityEntry.CurrentValues))
			{
				var validationContext = new ValidationContext(entity);
				Validator.ValidateObject(entity, validationContext);

				EntityEntry.State = EntityEntryState.Updated;
				var definition = EntityMapping.GetOrCreateDefinition(typeof(TEntity));
				var updateDefintion = UpdateDefinitionHelper.CreateFromDiff<TEntity>(EntityEntry.OriginalValues, EntityEntry.CurrentValues);
				yield return new UpdateOneModel<TEntity>(definition.CreateIdFilterFromEntity(entity), updateDefintion);
			}
			else
			{
				EntityEntry.State = EntityEntryState.NoChanges;
			}
		}
	}
}
