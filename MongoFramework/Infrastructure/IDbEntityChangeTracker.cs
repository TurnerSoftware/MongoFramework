namespace MongoFramework.Infrastructure
{
	public interface IDbEntityChangeTracker<TEntity> : IDbEntityCollection<TEntity>
	{
		void DetectChanges();
		void CommitChanges();
	}
}
