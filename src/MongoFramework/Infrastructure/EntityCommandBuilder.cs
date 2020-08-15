using System;
using System.Collections.Generic;
using System.Text;
using MongoFramework.Infrastructure.Commands;
using MongoFramework.Infrastructure.Mutation;

namespace MongoFramework.Infrastructure
{
	public static class EntityCommandBuilder<TEntity> where TEntity : class
	{
		public static IWriteCommand<TEntity> CreateCommand(EntityEntry entityEntry)
		{
			if (entityEntry.State == EntityEntryState.Added)
			{
				return new AddEntityCommand<TEntity>(entityEntry);
			}
			else if (entityEntry.State == EntityEntryState.Updated || entityEntry.State == EntityEntryState.NoChanges)
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
