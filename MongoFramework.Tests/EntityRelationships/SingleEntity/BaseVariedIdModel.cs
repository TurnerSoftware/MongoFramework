using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace MongoFramework.Tests.EntityRelationships.SingleEntity
{
	public class BaseVariedIdModel
	{
		public string Id { get; set; }

		[ForeignKey("GuidProperty")]
		public Guid GuidTestId { get; set; }
		public GuidIdModel GuidProperty { get; set; }

		[ForeignKey("ObjectIdProperty")]
		public ObjectId ObjectIdTestId { get; set; }
		public ObjectIdIdModel ObjectIdProperty { get; set; }
	}
}
