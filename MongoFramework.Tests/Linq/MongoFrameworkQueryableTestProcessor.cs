using MongoFramework.Infrastructure.Linq;
using System;
using System.Linq;

namespace MongoFramework.Tests.Linq
{
	public class MongoFrameworkQueryableTestProcessor<TEntity> : ILinqProcessor<TEntity>
	{
		public bool EntityProcessed { get; private set; }

		public void ProcessEntity(TEntity entity)
		{
			EntityProcessed = true;
		}
	}
}
