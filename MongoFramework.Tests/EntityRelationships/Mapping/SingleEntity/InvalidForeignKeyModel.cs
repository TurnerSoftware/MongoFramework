using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MongoFramework.Tests.EntityRelationships.Mapping.SingleEntity
{
	public class InvalidForeignKeyModel
	{
		public string Id { get; set; }

		[ForeignKey("Created_By")]
		public string CreatedById { get; set; }
		public virtual UserEntityModel CreatedBy { get; set; }
	}
}
