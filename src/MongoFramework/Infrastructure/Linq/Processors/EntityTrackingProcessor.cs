namespace MongoFramework.Infrastructure.Linq.Processors
{
	public class EntityTrackingProcessor<TEntity> : ILinqProcessor<TEntity> where TEntity : class
	{
		public IEntityChangeTracker<TEntity> ChangeTracker { get; private set; }

		public EntityTrackingProcessor(IEntityChangeTracker<TEntity> changeSet)
		{
			ChangeTracker = changeSet;
		}

		public void ProcessEntity(TEntity entity)
		{
			ChangeTracker.Update(entity, EntityEntryState.NoChanges);
		}
	}
}
