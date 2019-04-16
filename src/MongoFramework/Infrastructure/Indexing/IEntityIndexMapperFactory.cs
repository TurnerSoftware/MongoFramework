using System;

namespace MongoFramework.Infrastructure.Indexing
{
	public interface IEntityIndexMapperFactory
	{
		IEntityIndexMapper GetIndexMapper(Type entityType);
	}
}
