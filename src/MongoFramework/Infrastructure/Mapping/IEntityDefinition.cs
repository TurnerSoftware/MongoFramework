using System;
using System.Collections.Generic;

namespace MongoFramework.Infrastructure.Mapping
{
	public interface IEntityDefinition
	{
		Type EntityType { get; set; }
		string CollectionName { get; set; }
		IEnumerable<IEntityProperty> Properties { get; set; }
		IEnumerable<IEntityRelationship> Relationships { get; set; }
		IEnumerable<IEntityIndex> Indexes { get; set; }
	}
}
