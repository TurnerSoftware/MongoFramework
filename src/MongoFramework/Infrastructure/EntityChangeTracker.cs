using System.Linq;

namespace MongoFramework.Infrastructure
{
	public class EntityChangeTracker<TEntity> : EntityCollection<TEntity>, IEntityChangeTracker<TEntity> where TEntity : class
	{
		public void DetectChanges()
		{
			var entries = Entries.Where(e => e.State == EntityEntryState.NoChanges || e.State == EntityEntryState.Updated);

			foreach (var entry in entries)
			{
				entry.State = entry.HasChanges() ? EntityEntryState.Updated : EntityEntryState.NoChanges;
			}
		}

		public void CommitChanges()
		{
			foreach (var entry in Entries.ToArray())
			{
				if (entry.State == EntityEntryState.Added || entry.State == EntityEntryState.Updated)
				{
					entry.Refresh();
				}
				else if (entry.State == EntityEntryState.Deleted)
				{
					Entries.Remove(entry);
				}
			}
		}
	}
}