using System.Collections.Generic;

namespace MongoFramework.Infrastructure.Linq
{
	public class EntityProcessorCollection<TEntity> : List<ILinqProcessor<TEntity>>, ILinqProcessor<TEntity> where TEntity : class
	{
		public void ProcessEntity(TEntity entity, IMongoDbConnection connection)
		{
			foreach (var processor in this)
			{
				processor.ProcessEntity(entity, connection);
			}
		}
	}
}
