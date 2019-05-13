using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoFramework.Infrastructure.Commands;

namespace MongoFramework.Infrastructure
{
	public interface ICommandWriter<TEntity> where TEntity : class
	{
		void Write(IEnumerable<IWriteCommand<TEntity>> writeCommands);
		Task WriteAsync(IEnumerable<IWriteCommand<TEntity>> writeCommands, CancellationToken cancellationToken = default(CancellationToken));
	}
}
