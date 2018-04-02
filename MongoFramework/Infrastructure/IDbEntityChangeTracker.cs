namespace MongoFramework.Infrastructure
{
	public interface IDbEntityChangeTracker
	{
		void DetectChanges();
		void CommitChanges();
	}

	public interface IDbEntityChangeTracker<TEntity> : IDbEntityChangeTracker, IDbEntityCollection<TEntity>
	{
	}
}
