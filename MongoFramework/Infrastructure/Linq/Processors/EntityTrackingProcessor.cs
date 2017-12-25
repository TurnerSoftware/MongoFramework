using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure.Linq.Processors
{
	public class EntityTrackingProcessor<TEntity> : ILinqProcessor<TEntity>
	{
		public IDbChangeTracker<TEntity> ChangeTracker { get; private set; }

		public EntityTrackingProcessor(IDbChangeTracker<TEntity> changeSet)
		{
			ChangeTracker = changeSet;
		}

		public void ProcessEntity(TEntity entity)
		{
			ChangeTracker.Update(entity, DbEntityEntryState.NoChanges);
		}
	}
}
