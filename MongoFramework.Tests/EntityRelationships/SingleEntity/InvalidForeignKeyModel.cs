using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
