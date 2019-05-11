using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;
using MongoFramework.Infrastructure.DefinitionHelpers;

namespace MongoFramework.Infrastructure.Commands
{
	public class UpdateEntityCommand<TEntity> : IWriteCommand<TEntity> where TEntity : class
	{
		private EntityEntry<TEntity> EntityEntry { get; }

		public UpdateEntityCommand(EntityEntry<TEntity> entityEntry)
		{
			EntityEntry = entityEntry;
		}

		public IEnumerable<WriteModel<TEntity>> GetModel()
		{
			//var idFieldValue = EntityMapper.GetIdValue(entry.Entity);
			//var filter = Builders<TEntity>.Filter.Eq(idFieldName, idFieldValue);
			//var updateDefintion = UpdateDefinitionHelper.CreateFromDiff<TEntity>(EntityEntry.OriginalValues, EntityEntry.CurrentValues);

			////MongoDB doesn't like it if an UpdateDefinition is empty.
			////This is primarily to work around a mutation that may set an entity to its default state.
			//if (updateDefintion.HasChanges())
			//{
			//	writeModel.Add(new UpdateOneModel<TEntity>(filter, updateDefintion));
			//}
			//yield return new InsertOneModel<TEntity>(EntityEntry.Entity);
			yield break;
		}
	}
}
