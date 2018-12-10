namespace MongoFramework.Infrastructure
{
	public enum EntityEntryState
	{
		/// <summary>
		/// There are no known changes to the entity
		/// </summary>
		NoChanges,
		/// <summary>
		/// The entity is known to be added during the next save
		/// </summary>
		Added,
		/// <summary>
		/// The entity is known to be updated during the next save
		/// </summary>
		Updated,
		/// <summary>
		/// The entity is known to be deleted during the next save
		/// </summary>
		Deleted
	}
}
