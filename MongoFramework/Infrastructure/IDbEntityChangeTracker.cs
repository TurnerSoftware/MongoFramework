namespace MongoFramework.Infrastructure
{
	public interface IDbEntityChangeTracker<TEntity> : IDbEntityContainer<TEntity>
	{
		void DetectChanges();
		void CommitChanges();
	}
}
