using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using MongoDB.Driver;

namespace MongoFramework.Infrastructure.Commands
{
	public class RemoveEntityRangeCommand<TEntity> : IWriteCommand<TEntity> where TEntity : class
	{
		private Expression<Func<TEntity, bool>> Predicate { get; }

		public RemoveEntityRangeCommand(Expression<Func<TEntity, bool>> predicate)
		{
			Predicate = predicate;
		}

		public IEnumerable<WriteModel<TEntity>> GetModel()
		{
			yield return new DeleteManyModel<TEntity>(Predicate);
		}
	}
}
