using MongoDB.Bson;
using System;

namespace MongoFramework.Tests.Infrastructure.EntityRelationships
{
	public class StringIdModel
	{
		public string Id { get; set; }
		public string SecondaryId { get; set; }
		public string Description { get; set; }
	}

	public class ObjectIdIdModel
	{
		public ObjectId Id { get; set; }
		public ObjectId SecondaryId { get; set; }
		public string Description { get; set; }
	}

	public class GuidIdModel
	{
		public Guid Id { get; set; }
		public Guid SecondaryId { get; set; }
		public string Description { get; set; }
	}
}
