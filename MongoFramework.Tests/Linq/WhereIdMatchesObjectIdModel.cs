using MongoDB.Bson;
using System;
using System.Linq;

namespace MongoFramework.Tests.Linq
{
	public class WhereIdMatchesObjectIdModel
	{
		public ObjectId Id { get; set; }
		public string Description { get; set; }
	}
}
