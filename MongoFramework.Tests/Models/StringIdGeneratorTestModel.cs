using System.ComponentModel.DataAnnotations;

namespace MongoFramework.Tests.Models
{
	public class StringIdGeneratorTestModel
	{
		[Key]
		public string MyCustomId { get; set; }
	}
}