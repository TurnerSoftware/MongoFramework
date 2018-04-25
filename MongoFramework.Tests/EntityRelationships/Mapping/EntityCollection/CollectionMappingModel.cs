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

		public virtual ICollection<StringIdModel> StringModelEntities { get; set; }
		public virtual ICollection<ObjectIdIdModel> ObjectIdModelEntities { get; set; }
		public virtual ICollection<GuidIdModel> GuidModelEntities { get; set; }

		[InverseProperty("SecondaryId")]
		public virtual ICollection<StringIdModel> InverseCollection { get; set; }
	}
}
