using MongoFramework.Infrastructure.Commands;

namespace MongoFramework.Infrastructure
{
	public static class EntityCommandBuilder<TEntity> where TEntity : class
	{
		public static IWriteCommand<TEntity> CreateCommand(EntityEntry<TEntity> entityEntry)
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
