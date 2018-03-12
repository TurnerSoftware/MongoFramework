using System.ComponentModel.DataAnnotations;

namespace MongoFramework.Tests.Models
{
	public class MongoDbSetValidationModel
	{
		public string Id { get; set; }

		[Required]
		public string RequiredField { get; set; }
	}
}