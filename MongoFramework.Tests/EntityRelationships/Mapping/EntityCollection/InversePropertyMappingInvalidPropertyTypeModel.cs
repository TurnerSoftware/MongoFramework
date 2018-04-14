using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MongoFramework.Tests.EntityRelationships.Mapping.EntityCollection
{
	public class InversePropertyMappingInvalidPropertyTypeModel
	{
		public string Id { get; set; }

		[InverseProperty("CreatedDate")]
		public ICollection<RelatedEntityModel> RelatedEntities { get; set; }
	}
}
