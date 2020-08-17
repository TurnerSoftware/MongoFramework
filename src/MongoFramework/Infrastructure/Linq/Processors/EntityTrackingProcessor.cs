namespace MongoFramework.Infrastructure.Linq.Processors
{
	public class EntityTrackingProcessor<TEntity> : ILinqProcessor<TEntity> where TEntity : class
	{
		public IMongoDbContext Context { get; }

		public EntityTrackingProcessor(IMongoDbContext context)
		{
			Context = context;
		}

		public void ProcessEntity(TEntity entity, IMongoDbConnection connection)
		{
			var entry = Context.ChangeTracker.GetEntry(entity);
			if (entry == null || entry.State == EntityEntryState.NoChanges)
			{
				Context.ChangeTracker.SetEntityState<TEntity>(entity, EntityEntryState.NoChanges);
			}
		}
	}
}
