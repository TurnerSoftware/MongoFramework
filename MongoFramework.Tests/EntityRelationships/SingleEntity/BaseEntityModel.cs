using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Tests.EntityRelationships.SingleEntity
{
	public class BaseEntityModel
	{
		public string Id { get; set; }

		[ForeignKey("CreatedBy")]
		public string CreatedById { get; set; }
		public UserEntityModel CreatedBy { get; set; }

		public string UpdatedById { get; set; }
		[ForeignKey("UpdatedById")]
		public UserEntityModel UpdatedBy { get; set; }
	}
}
