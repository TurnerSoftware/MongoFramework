﻿using MongoFramework.Bson;

namespace MongoFramework.Infrastructure
{
	public static class EntityEntryExtensions
	{
		/// <summary>
		/// Whether there are any changes between the original and current values of the entity.
		/// </summary>
		/// <returns></returns>
		public static bool HasChanges(this EntityEntry entry)
		{
			return BsonDiff.HasDifferences(entry.OriginalValues, entry.CurrentValues);
		}
	}
}
