using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Tests.EntityRelationships.SingleEntity
{
	public class UnsupportedIdModel
	{
		public string Id { get; set; }
		
		[ForeignKey("CreatedBy")]
		public int CreatedById { get; set; }
		public UserEntityModel CreatedBy { get; set; }
	}
}
