using System;
using System.Linq;

namespace MongoFramework.Tests.EntityRelationships.Mapping.EntityCollection
{
	public class RelatedEntityModel
	{
		public string Id { get; set; }
		public string RelatedId { get; set; }
		public string Description { get; set; }
	}
}
