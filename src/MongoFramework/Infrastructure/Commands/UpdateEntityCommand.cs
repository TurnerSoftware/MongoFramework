﻿using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;
using MongoFramework.Infrastructure.DefinitionHelpers;
using MongoFramework.Infrastructure.Mapping;

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
			var definition = EntityMapping.GetOrCreateDefinition(typeof(TEntity));
			var updateDefintion = UpdateDefinitionHelper.CreateFromDiff<TEntity>(EntityEntry.OriginalValues, EntityEntry.CurrentValues);

			//MongoDB doesn't like it if an UpdateDefinition is empty.
			//This is primarily to work around a mutation that may set an entity to its default state.
			if (updateDefintion.HasChanges())
			{
				yield return new UpdateOneModel<TEntity>(definition.CreateIdFilterFromEntity(EntityEntry.Entity as TEntity), updateDefintion);
			}
		}
	}
}
