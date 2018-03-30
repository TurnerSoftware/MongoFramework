using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace MongoFramework.Tests.Models
{
	public class ObjectIdGeneratorTestModel
	{
		[Key]
		public ObjectId MyCustomId { get; set; }
	}
}