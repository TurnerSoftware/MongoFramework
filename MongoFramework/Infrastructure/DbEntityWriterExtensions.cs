using MongoDB.Bson;
using MongoFramework.Infrastructure.Mutators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure
{
	public static class DbEntityWriterExtensions
	{
		/// <summary>
		/// Writes entries from the <see cref="IDbChangeTracker{TEntity}"/> to the database.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="writer"></param>
		/// <param name="changes"></param>
		public static void WriteChanges<TEntity>(this IDbEntityChangeWriter<TEntity> writer, IDbChangeTracker<TEntity> changes)
		{
			var entriesByState = changes.GetEntries().GroupBy(e => e.State);

			foreach (var group in entriesByState)
			{
				var entities = group.Select(e => e.Entity);

				if (group.Key == DbEntityEntryState.Added)
				{
					writer.AddRange(entities);
					changes.UpdateRange(entities, DbEntityEntryState.NoChanges);
				}
				else if (group.Key == DbEntityEntryState.Updated)
				{
					writer.UpdateRange(group);
					foreach (var entry in group)
					{
						entry.Refresh();
					}
				}
				else if (group.Key == DbEntityEntryState.Deleted)
				{
					writer.RemoveRange(entities);
					changes.RemoveRange(entities);
				}
			}
		}
		/// <summary>
		/// Writes entries from the <see cref="IDbChangeTracker{TEntity}"/> to the database.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="writer"></param>
		/// <param name="changes"></param>
		/// <returns></returns>
		public static async Task WriteChangesAsync<TEntity>(this IAsyncDbEntityChangeWriter<TEntity> writer, IDbChangeTracker<TEntity> changes)
		{
			var entriesByState = changes.GetEntries().GroupBy(e => e.State);

			foreach (var group in entriesByState)
			{
				var entities = group.Select(e => e.Entity);

				if (group.Key == DbEntityEntryState.Added)
				{
					await writer.AddRangeAsync(entities);
					changes.UpdateRange(entities, DbEntityEntryState.NoChanges);
				}
				else if (group.Key == DbEntityEntryState.Updated)
				{
					await writer.UpdateRangeAsync(group);
					foreach (var entry in group)
					{
						entry.Refresh();
					}
				}
				else if (group.Key == DbEntityEntryState.Deleted)
				{
					await writer.RemoveRangeAsync(entities);
					changes.RemoveRange(entities);
				}
			}
		}
	}
}
