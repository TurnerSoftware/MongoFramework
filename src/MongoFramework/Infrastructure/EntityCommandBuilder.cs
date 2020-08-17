using System;
using MongoFramework.Infrastructure.Commands;
using MongoFramework.Infrastructure.Internal;

namespace MongoFramework.Infrastructure
{
	public static class EntityCommandBuilder
	{
		public static IWriteCommand CreateCommand(EntityEntry entityEntry)
		{
			var entityType = entityEntry.EntityType;
			var method = GenericsHelper.GetMethodDelegate<Func<EntityEntry, IWriteCommand>>(
				typeof(EntityCommandBuilder), nameof(InternalCreateCommand), entityType
			);
			return method(entityEntry);
		}

		private static IWriteCommand<TEntity> InternalCreateCommand<TEntity>(EntityEntry entityEntry) where TEntity : class
		{
			if (entityEntry.State == EntityEntryState.Added)
			{
				return new AddEntityCommand<TEntity>(entityEntry);
			}
			else if (entityEntry.State == EntityEntryState.Updated)
			{
				return new UpdateEntityCommand<TEntity>(entityEntry);
			}
			else if (entityEntry.State == EntityEntryState.Deleted)
			{
				return new RemoveEntityCommand<TEntity>(entityEntry);
			}

			return null;
		}
	}
}
