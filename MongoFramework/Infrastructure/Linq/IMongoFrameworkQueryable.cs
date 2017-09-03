using MongoFramework.Infrastructure.Linq.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure.Linq
{
	public interface IMongoFrameworkQueryable<TEntity, TOutput> : IOrderedQueryable<TOutput>
	{
		EntityProcessorCollection<TEntity> EntityProcessors { get; }
	}
}
