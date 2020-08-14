
namespace MongoFramework
{
	public class BucketSetOptions : IDbSetOptions
	{
		public int BucketSize { get; set; }
		public string EntityTimeProperty { get; set; }
	}
}
