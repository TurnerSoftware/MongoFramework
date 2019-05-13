namespace MongoFramework.Infrastructure.Linq.Processors
{
	public class EntityTrackingProcessor<TEntity> : ILinqProcessor<TEntity> where TEntity : class
	{
		public IEntityCollection<TEntity> EntityCollection { get; private set; }

		public EntityTrackingProcessor(IEntityCollection<TEntity> collection)
		{
			EntityCollection = collection;
		}

		public void ProcessEntity(TEntity entity, IMongoDbConnection connection)
		{
			EntityCollection.Update(entity, EntityEntryState.NoChanges);
		}
	}
}
