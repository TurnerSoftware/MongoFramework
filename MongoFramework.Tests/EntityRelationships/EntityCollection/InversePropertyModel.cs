using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Tests.EntityRelationships.EntityCollection
{
	public class InversePropertyModel
	{
		public string Id { get; set; }

		[InverseProperty("RelatedId")]
		public ICollection<RelatedEntityModel> RelatedEntities { get; set; }
	}
}
