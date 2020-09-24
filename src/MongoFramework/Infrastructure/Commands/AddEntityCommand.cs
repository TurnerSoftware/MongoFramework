using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Driver;

namespace MongoFramework.Infrastructure.Commands
{
	public class AddEntityCommand<TEntity> : IWriteCommand<TEntity> where TEntity : class
	{
		private EntityEntry EntityEntry { get; }

		public Type EntityType => typeof(TEntity);

		public AddEntityCommand(EntityEntry entityEntry)
		{
			EntityEntry = entityEntry;
		}

		public IEnumerable<WriteModel<TEntity>> GetModel(WriteModelOptions options)
		{
			var entity = EntityEntry.Entity as TEntity;

			var validationContext = new ValidationContext(entity);
			Validator.ValidateObject(entity, validationContext);

			yield return new InsertOneModel<TEntity>(entity);
		}
	}
}
