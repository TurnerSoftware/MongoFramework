using System;
using System.Linq;

namespace MongoFramework.Tests.EntityRelationships
{
	public class GuidIdModel
	{
		public Guid Id { get; set; }
		public Guid SecondaryId { get; set; }
		public string Description { get; set; }
	}
}
