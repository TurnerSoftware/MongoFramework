using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoFramework.Infrastructure.Commands;

namespace MongoFramework.Infrastructure
{
	public interface IEntityWriterPipeline<TEntity> where TEntity : class
	{
		IMongoDbConnection Connection { get; }
		void AddCollection(IEntityCollectionBase<TEntity> collection);
		void RemoveCollection(IEntityCollectionBase<TEntity> collection);
		void StageCommand(IWriteCommand<TEntity> command);
		void ClearStaging();
		void Write();
		Task WriteAsync(CancellationToken cancellationToken = default);
	}
}
