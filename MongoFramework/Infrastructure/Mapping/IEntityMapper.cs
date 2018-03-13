using System;
using System.Collections.Generic;

namespace MongoFramework.Infrastructure.Mapping
{
	public interface IEntityMapper
	{
		Type EntityType { get; }
		string GetIdName();
		object GetIdValue(object entity);
		string GetCollectionName();
		IEnumerable<IEntityPropertyMap> GetEntityMapping();
		IEnumerable<IEntityPropertyMap> TraverseMapping();
	}
}
