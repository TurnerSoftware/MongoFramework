namespace MongoFramework.Infrastructure.Linq.Processors
{
	public class EntityTrackingProcessor<TEntity> : ILinqProcessor<TEntity> where TEntity : class
	{
		public IEntityCollection<TEntity> EntityCollection { get; private set; }

		public EntityTrackingProcessor(IEntityCollection<TEntity> collection) => EntityCollection = collection;

		public void ProcessEntity(TEntity entity, IMongoDbConnection connection)
		{
			var entry = EntityCollection.GetEntry(entity);
			if (entry == null || entry.State == EntityEntryState.NoChanges)
			{
				EntityCollection.Update(entity, EntityEntryState.NoChanges);
			}
		}
	}
}
