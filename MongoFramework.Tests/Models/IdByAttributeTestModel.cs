using System.ComponentModel.DataAnnotations;

namespace MongoFramework.Tests.Models
{
	public class IdByAttributeTestModel
	{
		[Key]
		public string MyCustomId { get; set; }
	}
}