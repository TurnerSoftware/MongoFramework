using System;

namespace MongoFramework.Tests.Models
{
	class CommonEntity
	{
		public string Id { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime UpdatedDate { get; set; }
		public string Description { get; set; }
	}
}