using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure.Indexing
{
	public interface IEntityIndexWriter<TEntity>
	{
		void ApplyIndexing();
		Task ApplyIndexingAsync();
	}
}
