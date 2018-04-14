using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MongoFramework.Tests.EntityRelationships.Mapping.EntityCollection
{
	public class InversePropertyNonExistantPropertyModel
	{
		public string Id { get; set; }

		[InverseProperty("NonExistantPropertyId")]
		public ICollection<RelatedEntityModel> RelatedEntities { get; set; }
	}
}
