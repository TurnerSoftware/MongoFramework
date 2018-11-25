namespace MongoFramework.Infrastructure.Linq.Processors
{
	public class EntityTrackingProcessor<TEntity> : ILinqProcessor<TEntity> where TEntity : class
	{
		public IDbEntityChangeTracker<TEntity> ChangeTracker { get; private set; }

		public EntityTrackingProcessor(IDbEntityChangeTracker<TEntity> changeSet)
		{
			ChangeTracker = changeSet;
		}

		public void ProcessEntity(TEntity entity)
		{
			ChangeTracker.Update(entity, DbEntityEntryState.NoChanges);
		}
	}
}
