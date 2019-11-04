using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MongoFramework.Infrastructure.Linq
{
	public interface IMongoFrameworkQueryable : IOrderedQueryable
	{
		string ToQuery();
	}

	public interface IMongoFrameworkQueryable<TOutput> : IMongoFrameworkQueryable, IOrderedQueryable<TOutput>
	{
		IAsyncEnumerable<TOutput> AsAsyncEnumerable(CancellationToken cancellationToken = default);
	}
}
