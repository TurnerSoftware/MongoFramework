using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure
{
	public class DbSetCollection : List<IMongoDbSet>
	{
		public virtual void SaveChanges()
		{
			foreach (var dbSet in this)
			{
				dbSet.SaveChanges();
			}
		}

		public virtual async Task SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			foreach (var dbSet in this)
			{
				await dbSet.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
			}
		}
	}
}
