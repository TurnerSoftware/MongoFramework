using System;
using System.Collections.Generic;
using System.Text;

namespace MongoFramework.Infrastructure.Mapping
{
	public interface IEntityMapperFactory
	{
		IEntityMapper GetEntityMapper(Type entityType);
	}
}
