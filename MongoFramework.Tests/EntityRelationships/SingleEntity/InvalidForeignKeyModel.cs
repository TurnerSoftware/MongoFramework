using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MongoFramework.Tests.EntityRelationships.SingleEntity
{
	public class InvalidForeignKeyModel
	{
		public string Id { get; set; }

		[ForeignKey("Created_By")]
		public string CreatedById { get; set; }
		public UserEntityModel CreatedBy { get; set; }
	}
}
