using System;
using System.Collections.Generic;
using System.Linq;

namespace MongoFramework.Tests.EntityRelationships.Mapping.EntityCollection
{
	public class BaseEntityModel
	{
		public string Id { get; set; }
		public string Description { get; set; }

		public ICollection<RelatedEntityModel> RelatedEntities { get; set; }
	}
}
