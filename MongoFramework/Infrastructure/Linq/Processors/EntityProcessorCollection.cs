using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure.Linq.Processors
{
	public class EntityProcessorCollection<TEntity> : List<ILinqProcessor<TEntity>>, ILinqProcessor<TEntity>
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
