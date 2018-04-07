using MongoFramework.Bson;
using MongoFramework.Infrastructure.EntityRelationships;

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
			if (BsonDiff.HasDifferences(entry.OriginalValues, entry.CurrentValues))
			{
				return true;
			}
			else
			{
				return EntityRelationshipHelper.CheckForNavigationPropertyChanges(entry.Entity);
			}
		}
	}
}
