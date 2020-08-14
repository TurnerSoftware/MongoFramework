using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoFramework.Infrastructure.Mapping
{
	public class EntityDefinition : IEntityDefinition
	{
		public Type EntityType { get; set; }
		public string CollectionName { get; set; }
		public IEntityKeyGenerator KeyGenerator { get; set; }
		public IEnumerable<IEntityProperty> Properties { get; set; } = Enumerable.Empty<IEntityProperty>();
		public IEnumerable<IEntityRelationship> Relationships { get; set; } = Enumerable.Empty<IEntityRelationship>();
		public IEnumerable<IEntityIndex> Indexes { get; set; } = Enumerable.Empty<IEntityIndex>();
	}
}
