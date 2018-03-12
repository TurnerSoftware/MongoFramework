using MongoFramework.Attributes;

namespace MongoFramework.Tests.Mutation.Mutators.CreatedDate
{
	public class InvalidAttributeUseModel
	{
		public string Id { get; set; }

		[CreatedDate]
		public string CreatedDate { get; set; }
	}
}