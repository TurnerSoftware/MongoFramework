using MongoFramework.Tests.Mapping.Processors.Hierarchy;

namespace MongoFramework.Tests.Models
{
	public class ChildTestModel : ParentTestModel
	{
		public string DeclaredProperty { get; set; }
	}
}