using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MongoFramework.Tests.EntityRelationships.EntityNavigationCollection
{
	public class InversePropertyModel
	{
		public string Id { get; set; }

		[InverseProperty("RelatedId")]
		public ICollection<RelatedEntityModel> RelatedEntities { get; set; }
	}
}
