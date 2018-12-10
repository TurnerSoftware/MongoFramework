using System.Collections.Generic;

namespace MongoFramework.Infrastructure.Linq.Processors
{
	public class EntityProcessorCollection<TEntity> : List<ILinqProcessor<TEntity>>, ILinqProcessor<TEntity> where TEntity : class
	{
		public void ProcessEntity(TEntity entity)
		{
			foreach (var processor in this)
			{
				processor.ProcessEntity(entity);
			}
		}
	}
}
