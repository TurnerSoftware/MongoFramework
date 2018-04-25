using System;
using System.Linq;

namespace MongoFramework.Tests.EntityRelationships
{
	public class StringIdModel
	{
		public string Id { get; set; }
		public string SecondaryId { get; set; }
		public string Description { get; set; }
	}
}
