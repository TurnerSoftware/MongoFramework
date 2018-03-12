
namespace MongoFramework.Tests.Models
{
	public class NestedTraverseMappingModel
	{
		public string PropertyOne { get; set; }
		public int PropertyTwo { get; set; }
		public InnerNestedTraverseMappingModel InnerModel { get; set; }
	}
}