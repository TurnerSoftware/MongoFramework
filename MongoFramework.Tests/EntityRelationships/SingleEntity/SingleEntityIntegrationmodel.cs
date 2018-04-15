using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MongoFramework.Tests.EntityRelationships.SingleEntity
{
	public class SingleEntityIntegrationModel
	{
		public string Id { get; set; }

		[ForeignKey("RelatedItem")]
		public string RelatedItemId { get; set; }
		public StringIdModel RelatedItem { get; set; }
	}
}
