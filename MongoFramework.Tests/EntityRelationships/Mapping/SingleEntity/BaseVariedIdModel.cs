using MongoDB.Bson;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MongoFramework.Tests.EntityRelationships.Mapping.SingleEntity
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
