using MongoFramework.Bson;

namespace MongoFramework.Infrastructure
{
	public static class DbEntityEntryExtensions
	{
		/// <summary>
		/// Whether there are any changes between the original and current values of the entity.
		/// </summary>
		/// <returns></returns>
		public static bool HasChanges<TEntity>(this DbEntityEntry<TEntity> entry)
		{
			return BsonDiff.HasDifferences(entry.OriginalValues, entry.CurrentValues);
		}
	}
}
