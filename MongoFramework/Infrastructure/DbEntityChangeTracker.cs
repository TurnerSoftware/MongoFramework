using System.Linq;

namespace MongoFramework.Infrastructure
{
	public class DbEntityChangeTracker<TEntity> : DbEntityCollection<TEntity>, IDbEntityChangeTracker<TEntity>
	{
		public void DetectChanges()
		{
			var entries = Entries.Where(e => e.State == DbEntityEntryState.NoChanges || e.State == DbEntityEntryState.Updated);

			foreach (var entry in entries)
			{
				entry.State = entry.HasChanges() ? DbEntityEntryState.Updated : DbEntityEntryState.NoChanges;
			}
		}

		public void CommitChanges()
		{
			foreach (var entry in Entries.ToArray())
			{
				if (entry.State == DbEntityEntryState.Added || entry.State == DbEntityEntryState.Updated)
				{
					entry.Refresh();
				}
				else if (entry.State == DbEntityEntryState.Deleted)
				{
					Entries.Remove(entry);
				}
			}
		}
	}
}