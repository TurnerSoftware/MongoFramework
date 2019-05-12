using System;
using System.Collections.Generic;
using System.Text;

namespace MongoFramework.Infrastructure.Mapping
{
	public class EntityDefinition : IEntityDefinition
	{
		public Type EntityType { get; set; }
		public string CollectionName { get; set; }
		public IEnumerable<IEntityProperty> Properties { get; set; }
		public IEnumerable<IEntityRelationship> Relationships { get; set; }
		public IEnumerable<IEntityIndex> Indexes { get; set; }
	}
}
