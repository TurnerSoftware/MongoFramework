using MongoDB.Bson;
using MongoDB.Driver;
using MongoFramework.Bson;
using MongoFramework.Infrastructure.DefinitionHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure
{
	public static class DbEntityEntryExtensions
	{
		/// <summary>
		/// Returns a document that represents the changes between the original and current values of the entity.
		/// </summary>
		/// <returns></returns>
		public static BsonDocument GetChanges<TEntity>(this DbEntityEntry<TEntity> entry)
		{
			var result = BsonDiff.GetDifferences(entry.OriginalValues, entry.CurrentValues);

			if (result.HasDifference)
			{
				return result.Difference.AsBsonDocument;
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Whether there are any changes between the original and current values of the entity.
		/// </summary>
		/// <returns></returns>
		public static bool HasChanges<TEntity>(this DbEntityEntry<TEntity> entry)
		{
			return BsonDiff.HasDifferences(entry.OriginalValues, entry.CurrentValues);
		}

		/// <summary>
		/// Get the changes between the originan and current values of the entity as an update definition.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="entry"></param>
		/// <returns></returns>
		public static UpdateDefinition<TEntity> GetUpdateDefinition<TEntity>(this DbEntityEntry<TEntity> entry)
		{
			if (entry.HasChanges())
			{
				return UpdateDefinitionHelper.CreateFromDiff<TEntity>(entry.OriginalValues, entry.CurrentValues);
			}
			return null;
		}
	}
}
