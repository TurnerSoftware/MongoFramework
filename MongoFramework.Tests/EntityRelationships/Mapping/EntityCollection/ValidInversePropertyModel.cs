using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MongoFramework.Tests.EntityRelationships.Mapping.EntityCollection
{
	public class ValidInversePropertyModel
	{
		public string Id { get; set; }

		[InverseProperty("SecondaryId")]
		public virtual ICollection<StringIdModel> StringModelEntities { get; set; }
	}
}
