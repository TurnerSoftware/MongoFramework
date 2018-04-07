using MongoDB.Bson;
using System;
using System.Linq;

namespace MongoFramework.Tests.EntityRelationships
{
	public class ObjectIdIdModel
	{
		public ObjectId Id { get; set; }
		public string Description { get; set; }
	}
}
