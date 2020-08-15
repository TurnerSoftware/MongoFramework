using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Mutation;

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
			var entity = EntityEntry.Entity as TEntity;
			EntityMutation<TEntity>.MutateEntity(entity, MutatorType.Insert);

			var validationContext = new ValidationContext(entity);
			Validator.ValidateObject(entity, validationContext);

			yield return new InsertOneModel<TEntity>(entity);
		}
	}
}
