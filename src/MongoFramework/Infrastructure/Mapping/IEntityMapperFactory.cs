using System;

namespace MongoFramework.Infrastructure.Mapping
{
	public interface IEntityMapperFactory
	{
		IEntityMapper GetEntityMapper(Type entityType);
	}
}
