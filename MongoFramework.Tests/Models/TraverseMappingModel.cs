namespace MongoFramework.Tests.Models
{
	public class TraverseMappingModel
	{
		public string Id { get; set; }
		public NestedTraverseMappingModel NestedModel { get; set; }
		public NestedTraverseMappingModel RepeatedType { get; set; }
		public TraverseMappingModel RecursionType { get; set; }
	}
}