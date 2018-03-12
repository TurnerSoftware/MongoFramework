namespace MongoFramework.Tests.Models
{
	public class InnerNestedTraverseMappingModel
	{
		public string InnerMostProperty { get; set; }
		public TraverseMappingModel NestedRecursionType { get; set; }
	}
}