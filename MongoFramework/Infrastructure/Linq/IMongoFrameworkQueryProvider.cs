using MongoDB.Driver.Linq;
using MongoFramework.Infrastructure.Linq.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure.Linq
{
	public interface IMongoFrameworkQueryProvider<TEntity, TOutput> : IQueryProvider
	{
		IMongoQueryable UnderlyingQueryable { get; }
		EntityProcessorCollection<TEntity> EntityProcessors { get; }
	}
}
