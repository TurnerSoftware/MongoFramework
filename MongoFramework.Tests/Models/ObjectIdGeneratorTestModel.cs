using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace MongoFramework.Tests.Models
{
	public class ObjectIdGeneratorTestModel
	{
		[Key]
		public ObjectId MyCustomId { get; set; }
	}
}