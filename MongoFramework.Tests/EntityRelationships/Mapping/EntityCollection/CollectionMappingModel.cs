using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MongoFramework.Tests.EntityRelationships.Mapping.EntityCollection
{
	public class CollectionMappingModel
	{
		public string Id { get; set; }
		public string Description { get; set; }

		public ICollection<StringIdModel> StringModelEntities { get; set; }
		public ICollection<ObjectIdIdModel> ObjectIdModelEntities { get; set; }
		public ICollection<GuidIdModel> GuidModelEntities { get; set; }

		[InverseProperty("SecondaryId")]
		public ICollection<StringIdModel> InverseCollection { get; set; }
	}
}
