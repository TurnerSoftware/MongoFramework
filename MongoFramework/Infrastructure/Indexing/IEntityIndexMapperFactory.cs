using System;
using System.Collections.Generic;
using System.Text;

namespace MongoFramework.Infrastructure.Indexing
{
	public interface IEntityIndexMapperFactory
	{
		IEntityIndexMapper GetIndexMapper(Type entityType);
	}
}
