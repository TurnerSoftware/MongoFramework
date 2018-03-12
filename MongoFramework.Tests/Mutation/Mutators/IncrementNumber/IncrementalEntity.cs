using MongoFramework.Attributes;

namespace MongoFramework.Tests.Mutation.Mutators.IncrementNumber
{
	public class IncrementalEntity
	{
		[IncrementNumber]
		public int ByDefault { get; set; }
		[IncrementNumber(true)]
		public int ByUpdateOnly { get; set; }
		[IncrementNumber(10)]
		public int ByTen { get; set; }
	}
}