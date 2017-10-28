using MongoFramework.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Tests.Models
{
	[Table("MyCustomCollection", Schema = "MySchema")]
	class AttributeEntity
	{
		[Key]
		public string MyCustomId { get; set; }

		[CreatedDate]
		public DateTime CreatedDate { get; set; }

		[UpdatedDate]
		public DateTime UpdatedDate { get; set; }

		[NotMapped]
		public bool MyUnmappedField { get; set; }
	}
}
