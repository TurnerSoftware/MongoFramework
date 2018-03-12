using System.ComponentModel.DataAnnotations.Schema;

namespace MongoFramework.Tests.Models
{
	public class NotMappedPropertiesModel
	{
		public string Id { get; set; }
		[NotMapped]
		public string NotMapped { get; set; }
	}
}