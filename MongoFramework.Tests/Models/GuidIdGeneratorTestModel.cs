using System;
using System.ComponentModel.DataAnnotations;

namespace MongoFramework.Tests.Models
{
	public class GuidIdGeneratorTestModel
	{
		[Key]
		public Guid MyCustomId { get; set; }
	}
}