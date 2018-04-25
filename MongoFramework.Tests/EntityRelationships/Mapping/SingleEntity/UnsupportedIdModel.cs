using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MongoFramework.Tests.EntityRelationships.Mapping.SingleEntity
{
	public class UnsupportedIdModel
	{
		public string Id { get; set; }

		[ForeignKey("CreatedBy")]
		public int CreatedById { get; set; }
		public virtual UserEntityModel CreatedBy { get; set; }
	}
}
