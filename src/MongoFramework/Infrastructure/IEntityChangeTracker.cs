namespace MongoFramework.Infrastructure
{
	public interface IEntityChangeTracker
	{
		void DetectChanges();
		void CommitChanges();
	}

	public interface IEntityChangeTracker<TEntity> : IEntityChangeTracker, IEntityCollection<TEntity> where TEntity : class
	{
	}
}
