using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure.Linq.Processors
{
	public interface IEntityProcessor<TEntity>
	{
		void ProcessEntity(TEntity entity);
	}
}
